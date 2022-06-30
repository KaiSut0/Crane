using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Crane.Core;
using Rhino;
using Rhino.Geometry;

namespace Crane.Constraints
{
    public class RigidEdge : Constraint
    {
        public RigidEdge() { }
        private static object lockObj = new object();
        public override SparseMatrixBuilder Jacobian(CMesh cMesh)
        {
            int rows = cMesh.Mesh.TopologyEdges.Count;
            int columns = cMesh.Mesh.Vertices.Count * 3;
            List<Tuple<int, int, double>> elements = new List<Tuple<int, int, double>>();

            Parallel.For(0, cMesh.Mesh.TopologyEdges.Count, i =>
            {
                IndexPair ind = cMesh.Mesh.TopologyEdges.GetTopologyVertices(i);
                Point3d a = cMesh.Vertices[ind.I];
                Point3d b = cMesh.Vertices[ind.J];

                for (int j = 0; j < 3; j++)
                {
                    double var1 = (a[j] - b[j]) / cMesh.EdgeLengthSquared[i];
                    int rind1 = i;
                    int cind1 = 3 * ind.I + j;
                    Tuple<int, int, double> element1 = Tuple.Create(rind1, cind1, var1);

                    double var2 = (b[j] - a[j]) / cMesh.EdgeLengthSquared[i];
                    int rind2 = i;
                    int cind2 = 3 * ind.J + j;
                    Tuple<int, int, double> element2 = Tuple.Create(rind2, cind2, var2);

                    lock (lockObj)
                    {
                        elements.Add(element1);
                        elements.Add(element2);
                    }
                }

            });
            return new SparseMatrixBuilder(rows, columns, elements);
        }
        public override double[] Error(CMesh cMesh)
        {
            int n = cMesh.Mesh.TopologyEdges.Count;
            double[] error_ = new double[n];
            for (int i = 0; i < n; i++)
            {
                IndexPair ind = cMesh.Mesh.TopologyEdges.GetTopologyVertices(i);
                Point3d a = cMesh.Vertices[ind.I];
                Point3d b = cMesh.Vertices[ind.J];
                error_[i] = (a.DistanceToSquared(b) / cMesh.EdgeLengthSquared[i] - 1) / 2;
            }
            return error_;
        }
    }
}
