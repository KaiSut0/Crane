using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MathNet.Numerics.LinearAlgebra;

namespace Crane.Core
{
    public abstract class Constraint
    {
        public abstract SparseMatrixBuilder Jacobian(CMesh cMesh);
        public abstract double[] Error(CMesh cMesh);
    }
}
