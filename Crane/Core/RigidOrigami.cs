﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using Crane.Constraints;
using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.LinearAlgebra.Double;
using MathNet.Numerics.LinearAlgebra.Factorization;
using MathNet.Numerics.LinearAlgebra.Storage;
using Rhino;
using Rhino.Geometry;
using Rhino.Geometry.Collections;

namespace Crane.Core
{
    public class RigidOrigami
    {
        #region Constructors
        public RigidOrigami() { }

        public RigidOrigami(RigidOrigami rigidOrigami)
        {
            this.CMesh = new CMesh(rigidOrigami.CMesh);
            this.RecordedMeshPoints = new List<Vector<double>>();
            foreach(var vec in rigidOrigami.RecordedMeshPoints)
            {
                this.RecordedMeshPoints.Add(Vector<double>.Build.DenseOfArray(vec.ToArray()));
            }
            this.Constraints = rigidOrigami.Constraints;
            this.Fold = false;
            this.UnFold = false;
            this.IsRigidMode = rigidOrigami.IsRigidMode;
            this.IsPanelFlatMode = rigidOrigami.IsPanelFlatMode;
            this.IsFoldBlockMode = rigidOrigami.IsFoldBlockMode;
            this.IsConstraintMode = rigidOrigami.IsConstraintMode;
            this.IsRecordMode = false;
            this.CGNRComputationSpeeds = new List<List<double>>();
            this.NRComputationSpeeds = new List<double>();
        }

        public RigidOrigami(CMesh cMesh, List<Constraint> constraints)
        {
            this.CMesh = new CMesh(cMesh);
            this.RecordedMeshPoints = new List<Vector<double>>();
            this.RecordedMeshPoints.Add(this.CMesh.MeshVerticesVector);
            this.Constraints = constraints;
            this.Fold = false;
            this.UnFold = false;
            this.IsRigidMode = false;
            this.IsPanelFlatMode = false;
            this.IsFoldBlockMode = false;
            this.IsConstraintMode = false;
            this.IsRecordMode = false;
            this.CGNRComputationSpeeds = new List<List<double>>();
            this.NRComputationSpeeds = new List<double>();
        }
        #endregion
        #region Properties
        public CMesh CMesh { get; set; }
        public List<Vector<double>> RecordedMeshPoints { get; set; }
        public List<Constraint> Constraints { get; set; }
        public bool Fold { get; set; }
        public bool UnFold { get; set; }
        public bool IsRigidMode { get; set; }
        public bool IsPanelFlatMode { get; set; }
        public bool IsFoldBlockMode { get; set; }
        public bool IsConstraintMode { get; set; }
        public bool IsRecordMode { get; set; }
        public SparseMatrix Jacobian { get; protected set; }
        public Vector<double> Error { get; protected set; }
        public double Residual { get; protected set; }
        public int MoveVertexIndex { get; set; }
        public List<int> MoveVertexIndices { get; set; }
        public List<List<double>> CGNRComputationSpeeds { get; private set; }
        public List<double> NRComputationSpeeds { get; private set; }
        public bool UseNative { get; set; }
        #endregion

        #region Private Members
        protected RigidEdge EdgeLength = new RigidEdge();
        protected FlatPanel FlatPanel = new FlatPanel();
        protected MountainIntersectPenalty MountainIntersectPenalty = new MountainIntersectPenalty();
        protected ValleyIntersectPenalty ValleyIntersectPenalty = new ValleyIntersectPenalty();
        #endregion

        protected void ComputeError()
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
                var mError = MountainIntersectPenalty.Error(CMesh);
                var vError = ValleyIntersectPenalty.Error(CMesh);
                if(mError !=null)
                    errorList.AddRange(mError.ToList());
                if(vError!=null)
                    errorList.AddRange(vError.ToList());
            }
            if (IsConstraintMode)
            {
                foreach(Constraint constraint in Constraints)
                {
                    errorList.AddRange(constraint.Error(this.CMesh).ToList());
                }
            }
            Error = Vector<double>.Build.DenseOfArray(errorList.ToArray());
        }
        protected void ComputeJacobian()
        {
            int n = this.CMesh.Mesh.Vertices.Count * 3;
            Jacobian =(SparseMatrix) SparseMatrix.Build.Sparse(1, n);
            if (IsRigidMode)
            {
                Jacobian =(SparseMatrix) Jacobian.Stack(EdgeLength.Jacobian(this.CMesh));
            }
            if (IsPanelFlatMode)
            {
                Jacobian =(SparseMatrix) Jacobian.Stack(FlatPanel.Jacobian(this.CMesh));
            }
            if (IsFoldBlockMode)
            {
                var mJacobian = MountainIntersectPenalty.Jacobian(CMesh);
                var vjacobian = ValleyIntersectPenalty.Jacobian(CMesh);
                if(mJacobian!=null)
                    Jacobian =(SparseMatrix) Jacobian.Stack(MountainIntersectPenalty.Jacobian(this.CMesh));
                if(vjacobian!=null)
                    Jacobian =(SparseMatrix) Jacobian.Stack(ValleyIntersectPenalty.Jacobian(this.CMesh));
            }
            if (IsConstraintMode)
            {
                foreach (Constraint constraint in Constraints)
                {
                    Jacobian =(SparseMatrix) Jacobian.Stack(constraint.Jacobian(this.CMesh));
                }
            }
        }
        protected SparseMatrix ComputeFoldAngleJacobian()
        {
            int rows = this.CMesh.InnerEdges.Count;
            int columns = this.CMesh.DOF;
            List<Tuple<int, int, double>> elements = new List<Tuple<int, int, double>>();

            this.CMesh.Mesh.FaceNormals.ComputeFaceNormals();

            MeshVertexList vert = this.CMesh.Mesh.Vertices;

            for (int e_ind = 0; e_ind < this.CMesh.InnerEdges.Count; e_ind++)
            {
                /// Register indices
                IndexPair edge_ind = this.CMesh.InnerEdges[e_ind];
                int u = edge_ind.I;
                int v = edge_ind.J;
                IndexPair face_ind = this.CMesh.FacePairs[e_ind];
                int P = face_ind.I;
                int Q = face_ind.J;

                MeshFace face_P = this.CMesh.Mesh.Faces[P];
                MeshFace face_Q = this.CMesh.Mesh.Faces[Q];
                int p = 0;
                int q = 0;
                for (int i = 0; i < 3; i++)
                {
                    if (!edge_ind.Contains(face_P[i]))
                    {
                        p = face_P[i];
                    }
                    if (!edge_ind.Contains(face_Q[i]))
                    {
                        q = face_Q[i];
                    }
                }
                /// Compute normals
                Vector3d normal_P = this.CMesh.Mesh.FaceNormals[P];
                Vector3d normal_Q = this.CMesh.Mesh.FaceNormals[Q];
                /// Compute h_P & cot_Pu
                Vector3d vec_up = vert[p] - vert[u];
                Vector3d vec_uv = vert[v] - vert[u];
                double sin_Pu = (Vector3d.CrossProduct(vec_up, vec_uv) / (vec_up.Length * vec_uv.Length)).Length;
                double cos_Pu = (vec_up * vec_uv) / (vec_up.Length * vec_uv.Length);
                double cot_Pu = cos_Pu / sin_Pu;
                double len_up = vec_up.Length;
                double h_P = len_up * sin_Pu;
                /// Compute cot_Pv
                Vector3d vec_vp = vert[p] - vert[v];
                Vector3d vec_vu = vert[u] - vert[v];
                double sin_Pv = (Vector3d.CrossProduct(vec_vp, vec_vu) / (vec_vp.Length * vec_vu.Length)).Length;
                double cos_Pv = (vec_vp * vec_vu) / (vec_vp.Length * vec_vu.Length);
                double cot_Pv = cos_Pv / sin_Pv;
                /// Compute h_Q & cot_Qu
                Vector3d vec_uq = vert[q] - vert[u];
                double sin_Qu = (Vector3d.CrossProduct(vec_uq, vec_uv) / (vec_uq.Length * vec_uv.Length)).Length;
                double cos_Qu = (vec_uq * vec_uv) / (vec_uq.Length * vec_uv.Length);
                double cot_Qu = cos_Qu / sin_Qu;
                double len_uq = vec_uq.Length;
                double h_Q = len_uq * sin_Qu;
                /// Compute cot_Qv
                Vector3d vec_vq = vert[q] - vert[v];
                double sin_Qv = (Vector3d.CrossProduct(vec_vq, vec_vu) / (vec_vq.Length * vec_vu.Length)).Length;
                double cos_Qv = vec_vq * vec_vu / (vec_vq.Length * vec_vu.Length);
                double cot_Qv = cos_Qv / sin_Qv;
                List<double> normal_P_list = new List<double>();
                List<double> normal_Q_list = new List<double>();
                normal_P_list.Add(normal_P.X);
                normal_P_list.Add(normal_P.Y);
                normal_P_list.Add(normal_P.Z);
                normal_Q_list.Add(normal_Q.X);
                normal_Q_list.Add(normal_Q.Y);
                normal_Q_list.Add(normal_Q.Z);
                /// Compute coefficients
                double co_pv = (-1 * cot_Pv) / (cot_Pu + cot_Pv);
                double co_qv = (-1 * cot_Qv) / (cot_Qu + cot_Qv);
                double co_pu = (-1 * cot_Pu) / (cot_Pu + cot_Pv);
                double co_qu = (-1 * cot_Qu) / (cot_Qu + cot_Qv);
                /// Compute Jacobian
                for (int v_ind = 0; v_ind < vert.Count; v_ind++)
                {
                    if (v_ind == p)
                    {
                        for (int x = 0; x < 3; x++)
                        {
                            double var = normal_P_list[x] / h_P;
                            int rind = e_ind;
                            int cind = 3 * v_ind + x;
                            Tuple<int, int, double> element = Tuple.Create(rind, cind, var);
                            elements.Add(element);
                        }
                    }
                    if (v_ind == q)
                    {
                        for (int x = 0; x < 3; x++)
                        {
                            double var = normal_Q_list[x] / h_Q;
                            int rind = e_ind;
                            int cind = 3 * v_ind + x;
                            Tuple<int, int, double> element = Tuple.Create(rind, cind, var);
                            elements.Add(element);
                        }
                    }
                    if (v_ind == u)
                    {
                        for (int x = 0; x < 3; x++)
                        {
                            double var = co_pv * (normal_P_list[x] / h_P) + co_qv * (normal_Q_list[x] / h_Q);
                            int rind = e_ind;
                            int cind = 3 * v_ind + x;
                            Tuple<int, int, double> element = Tuple.Create(rind, cind, var);
                            elements.Add(element);
                        }
                    }
                    if (v_ind == v)
                    {
                        for (int x = 0; x < 3; x++)
                        {
                            double var = co_pu * (normal_P_list[x] / h_P) + co_qu * (normal_Q_list[x] / h_Q);
                            int rind = e_ind;
                            int cind = 3 * v_ind + x;
                            Tuple<int, int, double> element = Tuple.Create(rind, cind, var);
                            elements.Add(element);
                        }
                    }
                }
            }

            

            SparseMatrix foldAngleJacobian =(SparseMatrix)SparseMatrix.Build.SparseOfIndexed(rows, columns, elements);

            return foldAngleJacobian;
        }
        protected SparseMatrix ComputeFoldMotionMatrix(SparseMatrix foldAngleJacobian, SparseMatrix jacobian, double weight)
        {
            SparseMatrix foldJacobianSquared = (SparseMatrix) foldAngleJacobian.Transpose() * foldAngleJacobian;
            SparseMatrix jacobianSquared = (SparseMatrix) jacobian.Transpose() * jacobian;
            SparseMatrix foldMotionMatrix = (1 / weight) * foldJacobianSquared + ((weight - 1) / weight) * jacobianSquared;
            return foldMotionMatrix;
        }
        protected Vector<double> ComputeFoldMotionVector(Matrix<double> foldAngleJacobian, Vector<double> initialFoldAngleVector)
        {
            Vector<double> foldMotionVector = foldAngleJacobian.Transpose() * initialFoldAngleVector;
            return foldMotionVector;
        }
        protected Vector<double> ComputeInitialFoldAngleVectorForFold(double foldspeed)
        {
            Vector<double> dv;
            List<double> foldang = new List<double>();
            foldang = this.CMesh.GetFoldAngles();
            double[] driving_force = new double[this.CMesh.InnerEdgeAssignment.Count];

            for (int i = 0; i < this.CMesh.InnerEdgeAssignment.Count; i++)
            {
                if (this.CMesh.InnerEdgeAssignment[i] == 'V')
                {
                    driving_force[i] = (double)(-foldspeed * Math.Cos(foldang[i] / 2));
                }
                else if (this.CMesh.InnerEdgeAssignment[i] == 'M')
                {
                    driving_force[i] = (double)(foldspeed * Math.Cos(foldang[i] / 2));
                }
                else
                {
                    driving_force[i] = 0;
                }
            }
            dv = DenseVector.OfArray(driving_force);

            return dv;
        }
        protected Vector<double> ComputeInitialFoldAngleVectorForUnFold(double foldspeed)
        {
            Vector<double> dv;
            List<double> foldang = new List<double>();
            foldang = this.CMesh.GetFoldAngles();
            double[] driving_force = new double[this.CMesh.InnerEdgeAssignment.Count];

            for (int i = 0; i < this.CMesh.InnerEdgeAssignment.Count; i++)
            {
                if (this.CMesh.InnerEdgeAssignment[i] == 'V')
                {
                    driving_force[i] = (double)(foldspeed * Math.Cos(0.5*(Math.PI - Math.Abs(foldang[i]))));
                }
                else if (this.CMesh.InnerEdgeAssignment[i] == 'M')
                {
                    driving_force[i] = (double)(-foldspeed * Math.Cos(0.5*(Math.PI - Math.Abs(foldang[i]))));
                }
                else
                {
                    driving_force[i] = 0;
                }
            }
            dv = DenseVector.OfArray(driving_force);

            return dv;
        }
        public Vector<double> ComputeFoldMotion(double foldSpeed, int iterationMax)
        {
            SparseMatrix foldJacobian = ComputeFoldAngleJacobian();
            ComputeJacobian();
            SparseMatrix A = ComputeFoldMotionMatrix(foldJacobian, Jacobian, 10);
            Vector<double> drivingForce = Vector<double>.Build.Dense(CMesh.InnerEdgeAssignment.Count);
            if (UnFold)
            {
                drivingForce = ComputeInitialFoldAngleVectorForUnFold(foldSpeed);
            }
            if (Fold)
            {
                drivingForce = ComputeInitialFoldAngleVectorForFold(foldSpeed);
            }
            Vector<double> b = ComputeFoldMotionVector(foldJacobian, drivingForce);
            //Vector<double> foldMotion = -CGNRSolveForSymmetricPositiveDefiniteMatrixNative(A, b, 1e-6, iterationMax);
            Vector<double> foldMotion = -CGNRSolveForSymmetricPositiveDefiniteMatrix(A, b, 1e-6, iterationMax);

            return foldMotion;
        }
        public Vector<double> ComputeGrabMotion(List<int> vertexIndices, List<Vector3d> grabForces)
        {
            int count = vertexIndices.Count;
            if (grabForces.Count == 0) return SparseVector.Build.Sparse(CMesh.DOF); 
            List<Tuple<int, double>> elements = new List<Tuple<int, double>>();
            for(int i = 0; i < count; i++)
            {
                Tuple<int, double> elementX = Tuple.Create(vertexIndices[i] * 3 + 0, grabForces[i].X);
                Tuple<int, double> elementY = Tuple.Create(vertexIndices[i] * 3 + 1, grabForces[i].Y);
                Tuple<int, double> elementZ = Tuple.Create(vertexIndices[i] * 3 + 2, grabForces[i].Z);
                elements.Add(elementX);
                elements.Add(elementY);
                elements.Add(elementZ);
            }
            Vector<double> grabMotion;
            if (elements.Count == 0)
            {
                grabMotion = SparseVector.Build.Sparse(CMesh.DOF);
            }
            else
            {
                grabMotion = SparseVector.Build.SparseOfIndexed(CMesh.DOF, elements);
            }
            return grabMotion;
        }

        public static Vector<double> CGNRSolveForRectangleMatrix(Matrix<double> jacobian, Vector<double> b, Vector<double> x, double threshold, int iterationMax, ref List<double> cgnrComputationSpeed)
        {
            var sw = new System.Diagnostics.Stopwatch();
            sw.Start();
            int iteration = 0;
            Matrix<double> jacTrans = jacobian.Transpose();
            Vector<double> r = b - jacobian * x;
            Vector<double> p = jacTrans * r;
            double alpha = 0;
            double beta = 0;
            int matSize = Math.Min(jacobian.ColumnCount, jacobian.RowCount);

            while((iteration < Math.Min(iterationMax, matSize + 1)) && r.L2Norm() > threshold)
            {
                Vector<double> JTr0 = jacTrans * r;
                Vector<double> Jp = jacobian * p;
                alpha = Math.Pow(JTr0.L2Norm(), 2) / Math.Pow(Jp.L2Norm(), 2);
                x += alpha * p;
                r = b - jacobian * x;
                Vector<double> JTr1 = jacTrans * r;
                beta = Math.Pow(JTr1.L2Norm(), 2) / Math.Pow(JTr0.L2Norm(), 2);
                p = JTr1 + beta * p;
                iteration++;
            }
            sw.Stop();
            cgnrComputationSpeed.Add(sw.ElapsedMilliseconds);
            return x;
        }
        protected Vector<double> CGNRSolveForSymmetricPositiveDefiniteMatrix(SparseMatrix A, Vector<double> b, double threshold, int iterationMax)
        {
            Vector<double> x = new DenseVector(A.ColumnCount);

            int iteration = 0;
            Vector<double> r = b;
            Vector<double> p = r;

            double alpha, beta;
            int matSize = Math.Min(A.ColumnCount, A.RowCount) + 1;

            while ((iteration < Math.Min(iterationMax, matSize)) && r.L2Norm() > threshold)
            {
                double rTr = Math.Pow(r.L2Norm(), 2);
                double pTAp = p * (A * p);

                alpha = rTr / pTAp;
                x = x + alpha * p;
                r = b - (DenseVector)(A * x);
                beta = Math.Pow(r.L2Norm(), 2) / rTr;
                p = r + beta * p;
                iteration++;
            }

            return x;
        }
        protected Vector<double> CGNRSolveForRectangleMatrixNative(Matrix<double> jacobian, Vector<double> b, Vector<double> x,
            double threshold, int iterationMax)
        {
            //throw new NotImplementedException();
            SparseCompressedRowMatrixStorage<double> storage =
                (SparseCompressedRowMatrixStorage<double>)jacobian.Storage;
            int n = storage.RowCount;
            int m = storage.ColumnCount;
            int[] csrRowPtr = storage.RowPointers;
            int[] csrColInd = storage.ColumnIndices;
            double[] csrVal = storage.Values;
            double[] answer = CGNR.CGNR.CGNRForRect(n, m, csrRowPtr, csrColInd, csrVal, b.ToArray(), x.ToArray(), threshold, iterationMax);
            return Vector<double>.Build.DenseOfArray(answer);
        }

        protected Vector<double> CGNRSolveForSymmetricPositiveDefiniteMatrixNative(Matrix<double> A, Vector<double> b,
            double threshold, int iterationMax)
        {
            
            //throw new NotImplementedException();
            SparseCompressedRowMatrixStorage<double> storage =
                (SparseCompressedRowMatrixStorage<double>)A.Storage;
            int n = storage.RowCount;
            int[] csrRowPtr = storage.RowPointers;
            int[] csrColInd = storage.ColumnIndices;
            double[] csrVal = storage.Values;
            double[] answer = CGNR.CGNR.CGNRForSymmetricPositiveDefinite(n, csrRowPtr, csrColInd, csrVal, b.ToArray(), threshold, iterationMax);
            return Vector<double>.Build.DenseOfArray(answer);

        }
        protected void LinearSearch(Vector<double> vector)
        {
            double goldenRatio = (Math.Sqrt(5) - 1) / 2;
            Vector<double> nowCoordinates = this.CMesh.MeshVerticesVector;

            // Compute upper and lower bound
            Vector<double> lb = 0 * vector;
            Vector<double> ub = 1 * vector;

            Vector<double> x1, x2; // 内分点（「１：goldenRatio」と「goldenRatio：１」に分割する点）
            double e1, e2; // 内分点での誤差値

            x1 = 1 / (goldenRatio + 1) * (goldenRatio * lb + ub);
            Vector<double> updateVector = nowCoordinates + x1;
            CMesh.UpdateMesh(updateVector);
            ComputeError();
            e1 = Error.L2Norm();

            x2 = 1 / (goldenRatio + 1) * (lb + goldenRatio * ub);
            updateVector = nowCoordinates + x2;

            this.CMesh.UpdateMesh(updateVector);
            ComputeError();
            e2 = Error.L2Norm();

            for (int i = 0; i < 5; i++)
            {
                if(e1 < e2)
                {
                    ub = x2;
                    x2 = x1;
                    e2 = e1;
                    x1 = (1 / (goldenRatio + 1)) * (ub - lb) + lb;
                    updateVector = nowCoordinates + x1;
                    this.CMesh.UpdateMesh(updateVector);
                    ComputeError();
                    e1 = Error.L2Norm();
                }
                else
                {
                    lb = x1;
                    x1 = x2;
                    e1 = e2;
                    x2 = (1 / goldenRatio) * (ub - lb) + lb;
                    updateVector = nowCoordinates + x2;
                    this.CMesh.UpdateMesh(updateVector);
                    ComputeError();
                    e2 = Error.L2Norm();
                }
            }
            Vector<double> newCoordinates = nowCoordinates + 0.5 * (lb + ub);
            this.CMesh.UpdateMesh(newCoordinates);
            ComputeJacobian();
            ComputeError();

        }

        public double ComputeResidual()
        {
            ComputeError();
            return Error * Error / (Error.Count * Error.Count);
        }
        public double[] ComputeSvdOfJacobian()
        {
            ComputeJacobian();
            var mat = Matrix<double>.Build.DenseOfMatrix(Jacobian);
            Svd<double> svd = mat.Svd(false);
            return svd.S.ToArray();
        }
        public double NRSolve(Vector<double> initialMoveVector, double threshold, int iterationMaxNewtonMethod, int iterationMaxCGNR)
        {
            ComputeJacobian();
            ComputeError();
            Residual = Error * Error / (Error.Count * Error.Count);
            var cgnrComp = new List<double>();
            Vector<double> constrainedMoveVector = Vector<double>.Build.Dense(3 * CMesh.Mesh.Vertices.Count);
            if(initialMoveVector.L2Norm() != 0)
            {
                constrainedMoveVector = -CGNRSolveForRectangleMatrixNative(Jacobian, Error, initialMoveVector, Residual/100, Math.Min(Math.Min(Jacobian.RowCount, Jacobian.ColumnCount),iterationMaxCGNR));
                //constrainedMoveVector = -CGNRSolveForRectangleMatrix(Jacobian, Error, initialMoveVector, Residual/100, Math.Min(Math.Min(Jacobian.RowCount, Jacobian.ColumnCount),iterationMaxCGNR), ref cgnrComp);
                LinearSearch(constrainedMoveVector);
                Residual = Error * Error / (Error.Count * Error.Count);
            }
            //Vector<double> constrainedMoveVector = -CGNRSolveForRectangleMatrixNative(Jacobian, Error, initialMoveVector, Residual / 100, 20);
            //this.CMesh.MeshVerticesVector += constrainedMoveVector;
            //this.CMesh.UpdateMesh();
            int iteration = 0;
            cgnrComp = new List<double>();
            double nrComp = 0;
            var nrSw = new System.Diagnostics.Stopwatch();
            nrSw.Start();
            var foldAngles = Vector<double>.Build.Dense(CMesh.GetFoldAngles().ToArray());
            var nowFoldAngles = Vector<double>.Build.Dense(foldAngles.Count);
            while(iteration < iterationMaxNewtonMethod && Residual > threshold)
            {
                Vector<double> zeroVector = SparseVector.Build.Sparse(this.CMesh.DOF);
                //constrainedMoveVector = -CGNRSolveForRectangleMatrix(Jacobian, Error, zeroVector, Residual/100, Math.Min(Jacobian.ColumnCount, Jacobian.RowCount), ref cgnrComp);
                //constrainedMoveVector = -CGNRSolveForRectangleMatrix(Jacobian, Error, zeroVector, Residual/100, Math.Min(Math.Min(Jacobian.RowCount, Jacobian.ColumnCount),iterationMaxCGNR), ref cgnrComp);
                constrainedMoveVector = -CGNRSolveForRectangleMatrixNative(Jacobian, Error, zeroVector, Residual/100, Math.Min(Math.Min(Jacobian.RowCount, Jacobian.ColumnCount),iterationMaxCGNR));
                LinearSearch(constrainedMoveVector);
                Residual = Error * Error / (Error.Count * Error.Count);

                nowFoldAngles = Vector<double>.Build.Dense(CMesh.GetFoldAngles().ToArray());
                CMesh.SetFoldingSpeed((nowFoldAngles - foldAngles).ToArray());
                foldAngles = 1.0 * nowFoldAngles;

                iteration++;
            }
            nrSw.Stop();
            nrComp = nrSw.ElapsedMilliseconds;
            NRComputationSpeeds.Add(nrComp);
            CGNRComputationSpeeds.Add(cgnrComp);

            if (this.IsRecordMode)
            {
                this.RecordedMeshPoints.Add(this.CMesh.MeshVerticesVector);
            }
            return Residual;
        }

        public void SaveMode(bool isRigidMode, bool isPanelFlatMode, bool isFoldBlockMode, bool isConstraintMode)
        {
            IsRigidMode = isRigidMode;
            IsPanelFlatMode = isPanelFlatMode;
            IsFoldBlockMode = isFoldBlockMode;
            IsConstraintMode = isConstraintMode;
        }
    }
}
