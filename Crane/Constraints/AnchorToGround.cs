using System;
using Crane.Core;
using Grasshopper.GUI.IconEditor;
using MathNet.Numerics.LinearAlgebra;

namespace Crane.Constraints

{
    public class AnchorToGround : Constraint
    {
        public AnchorToGround() { }
        public override SparseMatrixBuilder Jacobian(CMesh cMesh)
        {
            throw new NotImplementedException();
        }
        public override double[] Error(CMesh cMesh)
        {
            throw new NotImplementedException();
        }
    }
}

