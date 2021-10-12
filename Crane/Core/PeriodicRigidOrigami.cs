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
            if (IsQuadFlatMode)
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
        protected new void ComputeJacobian()
        {
            int n = this.CMesh.Mesh.Vertices.Count * 3;
            Jacobian = (SparseMatrix)SparseMatrix.Build.Sparse(1, n);
            if (IsRigidMode)
            {
                Jacobian = (SparseMatrix)Jacobian.Stack(EdgeLength.Jacobian(this.CMesh));
            }
            if (IsQuadFlatMode)
            {
                Jacobian = (SparseMatrix)Jacobian.Stack(FlatPanel.Jacobian(this.CMesh));
            }
            if (IsFoldBlockMode)
            {
                Jacobian = (SparseMatrix)Jacobian.Stack(MountainIntersectPenalty.Jacobian(this.CMesh));
                Jacobian = (SparseMatrix)Jacobian.Stack(ValleyIntersectPenalty.Jacobian(this.CMesh));
            }
            if (IsConstraintMode)
            {
                foreach (Constraint constraint in Constraints)
                {
                    Jacobian = (SparseMatrix)Jacobian.Stack(constraint.Jacobian(this.CMesh));
                }
            }
            SparseMatrix rightJacobian = (SparseMatrix)SparseMatrix.Build.Sparse(Jacobian.RowCount, 4);
            Jacobian = (SparseMatrix)Jacobian.Append(rightJacobian);
            foreach(Constraint constraint in PeriodicConstraints)
            {
                Jacobian = (SparseMatrix)Jacobian.Stack(constraint.Jacobian(this.CMesh));
            }
        }
        public new double NRSolve(Vector<double> initialMoveVector, double threshold, int iterationMax)
        {
            ComputeJacobian();
            ComputeError();
            var cgnrComp = new List<double>();
            Vector<double> constrainedMoveVector = -CGNRSolveForRectangleMatrix(Jacobian, Error, initialMoveVector, 1e-10, 100, ref cgnrComp);
            this.CMesh.ConfigulationVector += constrainedMoveVector;
            this.CMesh.UpdateMesh();
            int iteration = 0;
            double residual = Error*Error;
            while(iteration < iterationMax && residual > threshold)
            {
                Vector<double> zeroVector = SparseVector.Build.Sparse(this.CMesh.DOF + 4);
                constrainedMoveVector = -CGNRSolveForRectangleMatrix(Jacobian, Error, zeroVector, 1e-15, Error.Count, ref cgnrComp);
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
