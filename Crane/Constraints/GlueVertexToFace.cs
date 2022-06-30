using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Crane.Core;
using MathNet.Numerics.LinearAlgebra;

namespace Crane.Constraints
{
    public class GlueVertexToFace : Constraint
    {
        public override double[] Error(CMesh cMesh)
        {
            throw new NotImplementedException();
        }

        public override SparseMatrixBuilder Jacobian(CMesh cMesh)
        {
            throw new NotImplementedException();
        }
    }
}
