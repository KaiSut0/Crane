using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Media3D;
using Crane.Core;
using MathNet.Numerics.LinearAlgebra;
using Rhino.Geometry;

namespace Crane.Constraints
{
    public class EqualBoundarySectorAngle : Constraint
    {
        public EqualBoundarySectorAngle(int[] primaryVertexIds, int[] secondaryVertexIds)
        {
            this.primaryVertexIds = primaryVertexIds;
            this.secondaryVertexIds = secondaryVertexIds;
        }
        private readonly int[] primaryVertexIds;
        private readonly int[] secondaryVertexIds;

        public override Matrix<double> Jacobian(CMesh cMesh)
        {
            List<Dictionary<int, Vector3d>> derivativeList = new List<Dictionary<int, Vector3d>>();
            int numPairs = primaryVertexIds.Length;
            int rows = numPairs;
            int columns = cMesh.DOF;
            List<Tuple<int, int, double>> elements = new List<Tuple<int, int, double>>();
            for (int i = 0; i < numPairs; i++)
            {
                var derivative = new Dictionary<int, Vector3d>();
                var pvId = primaryVertexIds[i];
                var svId = secondaryVertexIds[i];
                var pfs = cMesh.Mesh.TopologyVertices.ConnectedFaces(pvId);
                var sfs = cMesh.Mesh.TopologyVertices.ConnectedFaces(svId);
                SetDerivative(cMesh, pvId, pfs, ref derivative, true);
                SetDerivative(cMesh, svId, sfs, ref derivative, false);
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
            List<double> errors = new List<double>();
            int numPairs = primaryVertexIds.Length;
            for (int i = 0; i < numPairs; i++)
            {
                double error = 0;
                var pvId = primaryVertexIds[i];
                var svId = secondaryVertexIds[i];
                var pfs = cMesh.Mesh.TopologyVertices.ConnectedFaces(pvId);
                var sfs = cMesh.Mesh.TopologyVertices.ConnectedFaces(svId);
                var pAngleSum = SectorAngleSum(cMesh, pvId, pfs);
                var sAngleSum = SectorAngleSum(cMesh, svId, sfs);
                errors.Add(pAngleSum - sAngleSum);
            }
            return Vector<double>.Build.DenseOfArray(errors.ToArray());
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
