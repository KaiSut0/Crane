using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Crane.Core;
using MathNet.Numerics.LinearAlgebra;
using Rhino.Geometry;

namespace Crane.Constraints
{
    public class HoleVectorDevelopable : Constraint
    {
        public override Matrix<double> Jacobian(CMesh cMesh)
        {
            var pts = cMesh.Mesh.Vertices;
            List<Dictionary<int, Vector3d>> derivativeList1 = new List<Dictionary<int, Vector3d>>();
            List<Dictionary<int, Vector3d>> derivativeList2 = new List<Dictionary<int, Vector3d>>();
            int rows = 2 * cMesh.HoleLoopVertexIdsList.Count;
            int columns = cMesh.DOF;
            List<Tuple<int, int, double>> elements = new List<Tuple<int, int, double>>();

            foreach (var hole in cMesh.HoleLoopVertexIdsList)
            {
                var derivative1 = new Dictionary<int, Vector3d>();
                var derivative2 = new Dictionary<int, Vector3d>();
                var angleDerDicts = new Dictionary<int, Vector3d>[hole.Count - 1];
                var edges = new Vector3d[hole.Count - 1];
                for (int i = 0; i < hole.Count - 1; i++) edges[i] = pts[hole[i + 1]] - pts[hole[i]];
                var angles = new double[hole.Count - 1];

                for (int i = 1; i < hole.Count - 1; i++)
                {
                    double angle = -Math.PI;
                    var vId = hole[i];
                    var faces = cMesh.Mesh.TopologyVertices.ConnectedFaces(vId);
                    var angleDerDict = new Dictionary<int, Vector3d>();
                    foreach (var fId in faces)
                    {
                        var otherVIds = cMesh.GetOtherVertexIdPair(fId, vId);
                        int v1Id = otherVIds.I;
                        int v2Id = vId;
                        int v3Id = otherVIds.J;
                        angle += Utils.ComputeAngleFrom3Pts(pts[v1Id], pts[v2Id], pts[v3Id]);
                        var DSecDV = cMesh.ComputeDerivativeOfSectorAngle(fId, v1Id, v2Id, v3Id);
                        var vIds = new int[] { v1Id, v2Id, v3Id };
                        for (int j = 0; j < 3; j++)
                        {
                            if (angleDerDict.ContainsKey(vIds[j]))
                            {
                                angleDerDict[vIds[j]] += DSecDV[j];
                            }
                            else
                            {
                                angleDerDict[vIds[j]] = DSecDV[j];
                            }
                        }
                    }
                    angles[i] = angle;
                    angleDerDicts[i] = angleDerDict;
                }

                for (int i = 0; i < hole.Count - 1; i++)
                {
                    double angleSum = 0;
                    for (int j = 0; j < i + 1; j++)
                    {
                        angleSum += angles[j];
                    }

                    int v1 = hole[i];
                    int v2 = hole[i + 1];
                    var DLDv2 = edges[i] / edges[i].Length;
                    var DLDv1 = -DLDv2;
                    double cosAngleSum = Math.Cos(angleSum);
                    double sinAngleSum = Math.Sin(angleSum);
                    var DLDv = new Vector3d[] { DLDv1, DLDv2 };
                    var vs = new int[] { v1, v2 };
                    for (int j = 0; j < 2; j++)
                    {
                        if (derivative1.ContainsKey(vs[j]))
                        {
                            derivative1[vs[j]] += cosAngleSum * DLDv[j];
                            derivative2[vs[j]] += sinAngleSum * DLDv[j];
                        }
                        else
                        {
                            derivative1[vs[j]] = cosAngleSum * DLDv[j];
                            derivative2[vs[j]] = sinAngleSum * DLDv[j];
                        }
                    }

                    for (int j = 1; j < i + 1; j++)
                    {
                        var angleDerDict = angleDerDicts[j];
                        var keys = angleDerDict.Keys.ToArray();
                        foreach (var key in keys)
                        {
                            if (derivative1.ContainsKey(key))
                            {
                                derivative1[key] += edges[i].Length * (-sinAngleSum) * angleDerDict[key];
                                derivative2[key] += edges[i].Length * (cosAngleSum) * angleDerDict[key];
                            }
                            else
                            {
                                derivative1[key] = edges[i].Length * (-sinAngleSum) * angleDerDict[key];
                                derivative2[key] = edges[i].Length * (cosAngleSum) * angleDerDict[key];
                            }
                        }
                    }
                    derivativeList1.Add(derivative1);
                    derivativeList2.Add(derivative2);

                }

            }

            for (int i = 0; i < rows / 2; i++)
            {
                var derivative1 = derivativeList1[i];
                var derivative2 = derivativeList2[i];
                var keys = derivative1.Keys.ToArray();
                foreach (var key in keys)
                {
                    var der1 = derivative1[key];
                    var der2 = derivative2[key];
                    for (int j = 0; j < 3; j++)
                    {
                        var val1 = der1[j];
                        var val2 = der2[j];
                        elements.Add(Tuple.Create(2*i, 3*key+j, val1));
                        elements.Add(Tuple.Create(2*i + 1, 3*key+j, val2));
                    }
                }
            }

            return Matrix<double>.Build.SparseOfIndexed(rows, columns, elements);
        }

        public override Vector<double> Error(CMesh cMesh)
        {
            var pts = cMesh.Mesh.Vertices;
            List<double> errors = new List<double>();
            foreach (var hole in cMesh.HoleLoopVertexIdsList)
            {
                var edgeLengths = new double[hole.Count - 1];
                for (int i = 0; i < hole.Count - 1; i++) edgeLengths[i] = (pts[hole[i + 1]] - pts[hole[i]]).Length;
                var angles = new double[hole.Count - 1];
                for (int i = 1; i < hole.Count - 1; i++)
                {
                    double angle = -Math.PI;
                    var vId = hole[i];
                    var faces = cMesh.Mesh.TopologyVertices.ConnectedFaces(vId);
                    foreach (var fId in faces)
                    {
                        var otherVIds = cMesh.GetOtherVertexIdPair(fId, vId);
                        int v1Id = otherVIds.I;
                        int v2Id = vId;
                        int v3Id = otherVIds.J;
                        angle += Utils.ComputeAngleFrom3Pts(pts[v1Id], pts[v2Id], pts[v3Id]);
                    }
                    angles[i] = angle;
                }

                double vx = 0;
                double vy = 0;
                for (int i = 0; i < hole.Count - 1; i++)
                {
                    double rot = 0;
                    for (int j = 0; j < i+1; j++)
                    {
                        rot += angles[j];
                    }

                    vx += edgeLengths[i] * Math.Cos(rot);
                    vy += edgeLengths[i] * Math.Sin(rot);
                }
                errors.Add(vx);
                errors.Add(vy);
            }

            return Vector<double>.Build.DenseOfArray(errors.ToArray());
        }
    }
}
