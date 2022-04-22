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
    public class FixBoundarySectorAngle : Constraint
    {
        public FixBoundarySectorAngle(int[] vertexIds, double[] goalSectorAngleSums)
        {
            this.vertexIds = vertexIds;
            this.goalSectorAngleSums = goalSectorAngleSums;
        }
        private int[] vertexIds;
        private double[] goalSectorAngleSums;
        public override SparseMatrixBuilder Jacobian(CMesh cMesh)
        { 
            List<Dictionary<int, Vector3d>> derivativeList = new List<Dictionary<int, Vector3d>>();
            int numPairs = vertexIds.Length;
            int rows = numPairs;
            int columns = cMesh.DOF;
            List<Tuple<int, int, double>> elements = new List<Tuple<int, int, double>>();
            for (int i = 0; i < numPairs; i++)
            {
                var derivative = new Dictionary<int, Vector3d>();
                var vId = vertexIds[i];
                var fs = cMesh.Mesh.TopologyVertices.ConnectedFaces(vId);
                SetDerivative(cMesh, vId, fs, ref derivative, true);
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

            return new SparseMatrixBuilder(rows, columns, elements);
        }

        public override double[] Error(CMesh cMesh)
        {
            List<double> errors = new List<double>();
            int numPairs = vertexIds.Length;
            for (int i = 0; i < numPairs; i++)
            {
                double error = 0;
                var vId = vertexIds[i];
                var fs = cMesh.Mesh.TopologyVertices.ConnectedFaces(vId);
                var pAngleSum = SectorAngleSum(cMesh, vId, fs);
                double goalSectorAngleSum = goalSectorAngleSums[i];
                errors.Add(pAngleSum - goalSectorAngleSum);
            }

            return errors.ToArray();
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
