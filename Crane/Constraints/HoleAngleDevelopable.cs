using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Crane.Core;
using MathNet.Numerics.LinearAlgebra;
using Rhino;
using Rhino.Geometry;

namespace Crane.Constraints
{
    public class HoleAngleDevelopable : Constraint
    {
        public HoleAngleDevelopable(){}
        public override Matrix<double> Jacobian(CMesh cMesh)
        {
            List<Dictionary<int, Vector3d>> derivativeList = new List<Dictionary<int, Vector3d>>();
            int rows = cMesh.HoleLoopVertexIdsList.Count;
            int columns = cMesh.DOF;
            List<Tuple<int, int, double>> elements = new List<Tuple<int, int, double>>();
            foreach (var hole in cMesh.HoleLoopVertexIdsList)
            {
                var derivative = new Dictionary<int, Vector3d>();
                for (int i = 0; i < hole.Count - 1; i++)
                {
                    var vId = hole[i];
                    var faces = cMesh.Mesh.TopologyVertices.ConnectedFaces(vId);
                    foreach (var fId in faces)
                    {
                        var otherVIds = cMesh.GetOtherVertexIdPair(fId, vId);
                        int v1Id = otherVIds.I;
                        int v2Id = vId;
                        int v3Id = otherVIds.J;
                        var DSecDV = cMesh.ComputeDerivativeOfSectorAngle(fId, v1Id, v2Id, v3Id);
                        var vIds = new int[] { v1Id, v2Id, v3Id };
                        for (int j = 0; j < 3; j++)
                        {
                            if (derivative.ContainsKey(vIds[j]))
                            {
                                derivative[vIds[j]] += DSecDV[j];
                            }
                            else
                            {
                                derivative[vIds[j]] = DSecDV[j];
                            }
                        }
                    }
                }
                derivativeList.Add(derivative);
            }

            for (int i = 0; i < rows; i++)
            {
                var derivative = derivativeList[i];
                var keys = derivative.Keys.ToArray();
                foreach (var key in keys)
                {
                    var DSecDV = derivative[key];
                    for (int j = 0; j < 3; j++)
                    {
                        var val = DSecDV[j];
                        elements.Add(Tuple.Create(i, 3*key+j, val));
                    }
                }
            }

            return Matrix<double>.Build.SparseOfIndexed(rows, columns, elements);
        }

        public override Vector<double> Error(CMesh cMesh)
        {
            var pts = cMesh.Mesh.Vertices;
            List<double> errors = new List<double>();
            if (cMesh.HoleLoopVertexIdsList.Count != 0)
            {
                foreach (var hole in cMesh.HoleLoopVertexIdsList)
                {
                    double error = - 2 * Math.PI;
                    for (int i = 0; i < hole.Count - 1; i++)
                    {
                        error -= Math.PI;
                        var vId = hole[i];
                        var faces = cMesh.Mesh.TopologyVertices.ConnectedFaces(vId);
                        foreach (var fId in faces)
                        {
                            var otherVIds = cMesh.GetOtherVertexIdPair(fId, vId);
                            int v1Id = otherVIds.I;
                            int v2Id = vId;
                            int v3Id = otherVIds.J;
                            error += Util.ComputeAngleFrom3Pts(pts[v1Id], pts[v2Id], pts[v3Id]);
                        }
                    }

                    errors.Add(error);
                }
            }

            return Vector<double>.Build.DenseOfArray(errors.ToArray());
        }
    }
}
