using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting.Metadata.W3cXsd2001;
using Crane.Constraints;
using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.LinearAlgebra.Double;

namespace Crane.Core
{
    public class PeriodicRigidOrigami : RigidOrigami
    {
        #region Constructors
        public PeriodicRigidOrigami() { }

        public PeriodicRigidOrigami(CMesh cMesh, List<Constraint> constraints, List<CylindricallyPeriodic> periodicConstraints) : base(cMesh, constraints) 
        {
            this.PeriodicConstraints = periodicConstraints;
            this.CMesh.IsPeriodic = true;
        }
        #endregion
        #region Properties
        public List<CylindricallyPeriodic> PeriodicConstraints { get; set; }
        #endregion

        #region Private Members
        #endregion

        protected new void ComputeError()
        {
            int n = this.CMesh.Mesh.Vertices.Count * 3;
            List<double> errorList = new List<double>();
            errorList.Add(0);
            if (IsRigidMode)
            {
                errorList.AddRange(EdgeLength.Error(this.CMesh).ToList());
            }
            if (IsPanelFlatMode)
            {
                errorList.AddRange(FlatPanel.Error(this.CMesh).ToList());
            }
            if (IsFoldBlockMode)
            {
                errorList.AddRange(MountainIntersectPenalty.Error(this.CMesh).ToList());
                errorList.AddRange(ValleyIntersectPenalty.Error(this.CMesh).ToList());
            }
            if (IsConstraintMode)
            {
                foreach(Constraint constraint in Constraints)
                {
                    errorList.AddRange(constraint.Error(this.CMesh).ToList());
                }
            }
            foreach(Constraint constraint in PeriodicConstraints)
            {
                errorList.AddRange(constraint.Error(this.CMesh).ToList());
            }
            Error = Vector<double>.Build.DenseOfArray(errorList.ToArray());
        }
        protected void ComputeJacobian()
        {
            int n = this.CMesh.Mesh.Vertices.Count * 3;
            SparseMatrixBuilder builder = new SparseMatrixBuilder(1, CMesh.DOF);
            if (IsRigidMode)
            {
                builder.Append(EdgeLength.Jacobian(CMesh));
            }
            if (IsPanelFlatMode)
            {
                builder.Append(FlatPanel.Jacobian(CMesh));
            }

            if (IsFoldBlockMode)
            {
                var mJacobian = MountainIntersectPenalty.Jacobian(CMesh);
                var vjacobian = ValleyIntersectPenalty.Jacobian(CMesh);
                if(mJacobian!=null) builder.Append(mJacobian);
                if(vjacobian!=null) builder.Append(vjacobian);
            }

            if (IsConstraintMode)
            {
                foreach (Constraint constraint in Constraints)
                {
                    builder.Append(constraint.Jacobian(CMesh));
                }
            }

            Jacobian = (SparseMatrix)Matrix<double>.Build.SparseOfIndexed(builder.Rows, builder.Columns, builder.Elements);
        }
        public new double NRSolve(Vector<double> initialMoveVector, double threshold, int iterationMax)
        {
            ComputeJacobian();
            ComputeError();
            var cgnrComp = new List<double>();
            Vector<double> constrainedMoveVector = -LinearAlgebra.Solve(Jacobian, Error, initialMoveVector, 1e-10, 100);
            this.CMesh.ConfigulationVector += constrainedMoveVector;
            this.CMesh.UpdateMesh();
            int iteration = 0;
            double residual = Error*Error;
            while(iteration < iterationMax && residual > threshold)
            {
                Vector<double> zeroVector = SparseVector.Build.Sparse(this.CMesh.DOF + 4);
                constrainedMoveVector = -LinearAlgebra.Solve(Jacobian, Error, zeroVector, threshold, Error.Count);
                //constrainedMoveVector = CGNRSolveForRectangleMatrix(Jacobian, Error, zeroVector, Error.L2Norm()/100, Error.Count);
                //LinearSearch(constrainedMoveVector);
                this.CMesh.ConfigulationVector += constrainedMoveVector;
                this.CMesh.UpdateMesh();
                ComputeError();
                ComputeJacobian();
                residual = Error*Error;
                iteration++;
            }
            if (this.IsRecordMode)
            {
                this.RecordedMeshPoints.Add(this.CMesh.MeshVerticesVector);
            }
            return residual;
        }
    }
}
