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
    public class EqualFoldAngle : Constraint
    {
        public EqualFoldAngle(CMesh cMesh, Line[] primaryEdges, Line[] secondaryEdges)
        {
            primaryEdgeIds = primaryEdges.Select(e => cMesh.GetInnerEdgeIndex(e)).ToArray();
            secondaryEdgeIds = secondaryEdges.Select(e => cMesh.GetInnerEdgeIndex(e)).ToArray();
        }
        private readonly int[] primaryEdgeIds;
        private readonly int[] secondaryEdgeIds;
        public override Matrix<double> Jacobian(CMesh cMesh)
        {
            throw new NotImplementedException();
        }

        public override Vector<double> Error(CMesh cMesh)
        {
            int rows = primaryEdgeIds.Length;
            var err = new double[rows];
            var foldAngles = cMesh.GetFoldAngles();

            for (int i = 0; i < rows; i++)
            {
                int id1 = primaryEdgeIds[i];
                int id2 = secondaryEdgeIds[i];
                double fa1 = foldAngles[id1];
                double fa2 = foldAngles[id2];
                err[i] = fa1 - fa2;
            }

            return Vector<double>.Build.DenseOfArray(err);
        }
    }
}
