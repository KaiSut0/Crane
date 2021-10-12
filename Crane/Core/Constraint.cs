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
        public abstract Matrix<double> Jacobian(CMesh cMesh);
        public abstract Vector<double> Error(CMesh cMesh);
    }
}
