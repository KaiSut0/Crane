using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Crane.Core;
using Grasshopper.Kernel;
using MathNet.Numerics.LinearAlgebra;
using Rhino.Geometry;

namespace Crane.Constraints
{
    public class SelectiveDevelopable : Constraint
    {
        public SelectiveDevelopable(CMesh cMesh, Point3d[] innerVertices, double tolerance)
        {
            var tmp = innerVertices.Select(vert => cMesh.GetInnerVertexId(vert, tolerance));
            var _innerVertexIds = new List<int>();
            foreach(int innerVertexId in tmp)
            {
                if (innerVertexId != -1) _innerVertexIds.Add(innerVertexId);
            }
            innerVertexIds = _innerVertexIds.ToArray();
            numInnerVerts = innerVertexIds.Length;
        }

        private readonly int[] innerVertexIds;
        private readonly int numInnerVerts;

        public override double[] Error(CMesh cMesh)
        {
            Mesh m = cMesh.Mesh;

            double[] err = new double[numInnerVerts];

            m.Normals.ComputeNormals();
            var topo = m.TopologyVertices;
            topo.SortEdges();

            for (int i = 0; i < numInnerVerts; i++)
            {
                int innerVertexId = innerVertexIds[i];
                Vector3d innerVertex = new Vector3d(topo[innerVertexId]);

                var neighborVertexIds = topo.ConnectedTopologyVertices(innerVertexId);


                double angleSum = 0;

                int n = neighborVertexIds.Length;

                var neighbors = neighborVertexIds.Select(id => new Vector3d(topo[id])).ToArray();
                var vecs = neighbors.Select(neighbor => neighbor - innerVertex).ToArray();

                for(int j = 0; j < n; j++)
                {
                    if (j == 0) angleSum += Vector3d.VectorAngle(vecs[n - 1], vecs[0]);
                    else angleSum += Vector3d.VectorAngle(vecs[j - 1], vecs[j]);
                }

                angleSum -= 2 * Math.PI;

                err[i] = angleSum;
            }

            return err;
        }

        public override SparseMatrixBuilder Jacobian(CMesh cMesh)
        {
            Mesh m = cMesh.Mesh;

            List<Tuple<int, int, double>> elements = new List<Tuple<int, int, double>>();

            var topo = m.TopologyVertices;
            topo.SortEdges();

            for(int i = 0; i < numInnerVerts; i++)
            {
                int innerVertexId = innerVertexIds[i];
                Vector3d innerVertex = new Vector3d(topo[innerVertexId]);

                var neighborVertexIds = topo.ConnectedTopologyVertices(innerVertexId);


                double angleSum = 0;

                int n = neighborVertexIds.Length;

                var neighbors = neighborVertexIds.Select(id => new Vector3d(topo[id])).ToArray();
                var vecs = neighbors.Select(neighbor => neighbor - innerVertex).ToArray();

                var normals = new Vector3d[n];
                for (int j = 0; j < n; j++)
                {
                    var tmp = new Vector3d();
                    if (j == 0) tmp = Vector3d.CrossProduct(vecs[n - 1], vecs[0]);
                    else tmp = Vector3d.CrossProduct(vecs[j - 1], vecs[j]);
                    tmp.Unitize();
                    normals[j] = tmp;
                }

                var vCenter = new Vector3d();

                for (int j = 0; j < n; j++)
                {
                    int neighborId = neighborVertexIds[j];
                    var v1 = Vector3d.CrossProduct(normals[j], vecs[j]);
                    v1 *= 1.0 / vecs[j].SquareLength;
                    var v2 = Vector3d.CrossProduct(normals[(j + 1) % n], vecs[j]);
                    v2 *= 1.0 / vecs[j].SquareLength;
                    v1 -= v2;
                    vCenter -= v1;

                    for (int k = 0; k < 3; k++)
                        elements.Add(new Tuple<int, int, double>(i, 3 * neighborId + k, v1[k]));
                }

                for (int k = 0; k < 3; k++)
                    elements.Add(new Tuple<int, int, double>(i, 3 * innerVertexId + k, vCenter[k]));
            }

            int rows = numInnerVerts;
            int columns = cMesh.DOF;
            return new SparseMatrixBuilder(rows, columns, elements);
        }
    }
}
