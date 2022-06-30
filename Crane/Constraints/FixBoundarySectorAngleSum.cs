using System;
using System.Collections.Generic;
using System.Linq;
using Crane.Core;
using Grasshopper.Kernel;
using MathNet.Numerics.LinearAlgebra;
using Rhino.Geometry;

namespace Crane.Constraints
{
    public class FixBoundarySectorAngleSum : Constraint
    {
        public FixBoundarySectorAngleSum(int[] vertexIds, double goalAngle)
        {
            this.vertexIds = vertexIds;
            this.goalAngle = goalAngle;
        }
        private readonly int[] vertexIds;
        private readonly double goalAngle;
        public override SparseMatrixBuilder Jacobian(CMesh cMesh)
        {
            int rows = 1;
            int columns = cMesh.DOF;
            List<Tuple<int, int, double>> elements = new List<Tuple<int, int, double>>();
            Dictionary<int, Vector3d> derivative = new Dictionary<int, Vector3d>();
            foreach (var vId in vertexIds)
            {
                var fs = cMesh.Mesh.TopologyVertices.ConnectedFaces(vId);
                SetDerivative(cMesh, vId, fs, ref derivative, true);
            }

            var keys = derivative.Keys.ToArray();
            foreach (var key in keys)
            {
                var DSecDV = derivative[key];
                for (int j = 0; j < 3; j++)
                {
                    var val = DSecDV[j];
                    elements.Add(Tuple.Create(0, 3*key+j, val));
                }
            }

            return new SparseMatrixBuilder(rows, columns, elements);
        }

        public override double[] Error(CMesh cMesh)
        {
            double angleSum = 0; 
            foreach (var vId in vertexIds)
            {
                var fs = cMesh.Mesh.TopologyVertices.ConnectedFaces(vId);
                angleSum += SectorAngleSum(cMesh, vId, fs);
                angleSum -= Math.PI;
            }

            double error = angleSum - goalAngle;
            return new double[] { error };
        }
        private double SectorAngleSum(CMesh cMesh, int vId, int[] faces)
        {
            var pts = cMesh.Mesh.Vertices;
            double angleSum = 0;
            foreach (var fId in faces)
            {
                var otherVIds = cMesh.GetOtherVertexIdPair(fId, vId);
                int v1Id = otherVIds.I;
                int v2Id = vId;
                int v3Id = otherVIds.J;
                angleSum += Util.ComputeAngleFrom3Pts(pts[v1Id], pts[v2Id], pts[v3Id]);
            }
            return angleSum;
        }

        private void SetDerivative(CMesh cMesh, int vId, int[] faces, ref Dictionary<int, Vector3d> derivative, bool primaryOrSecondary)
        {
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
                        if (primaryOrSecondary)
                        {
                            derivative[vIds[j]] += DSecDV[j];
                        }
                        else
                        {
                            derivative[vIds[j]] -= DSecDV[j];
                        }
                    }
                    else
                    {
                        if (primaryOrSecondary)
                        {
                            derivative[vIds[j]] = DSecDV[j];
                        }
                        else
                        {
                            derivative[vIds[j]] = -DSecDV[j];
                        }
                    }
                }
            }
        }

    }
}