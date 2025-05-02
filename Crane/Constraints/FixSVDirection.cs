using System;
using System.Collections.Generic;
using Crane.Core;
using MathNet.Numerics.LinearAlgebra;
using Rhino.Geometry;

namespace Crane.Constraints
{
    public class FixSVDirection : Constraint
    {
        public FixSVDirection(CMesh cMeshOrig, int[] faces, int[] Faces, double[] goalAngle, double strength) 
        {
            this.cMeshOrig = new CMesh(cMeshOrig);
            this.faces = faces;
            this.Faces = Faces;
            this.goalAngle = goalAngle;
            this.strength = strength;
        }
        public FixSVDirection(CMesh cMesh, CMesh cMeshOrig, Point3d[] facePoints, Point3d[] FacePoints, double[] goalAngle, double strength) 
        {
            this.cMeshOrig = new CMesh(cMeshOrig);
            
            this.faces = new int[facePoints.Length];
            this.Faces = new int[FacePoints.Length];
            for (int i = 0; i < facePoints.Length; i++)
            {
                faces[i] = cMesh.GetFaceIdFromFaceCenter(facePoints[i]);
                Faces[i] = cMesh.GetFaceIdFromFaceCenter(FacePoints[i]);
            }

            this.goalAngle = goalAngle;
            this.strength = strength;
        }


        private CMesh cMeshOrig;
        private int[] Faces;
        private int[] faces;
        private double strength;
        private double[] goalAngle;
 
        public override double[] Error(CMesh cMesh)
        {
            int n = faces.Length;
            double[] error = new double[n];
            var pts = cMesh.Mesh.Vertices.ToPoint3dArray();
            var Pts = cMeshOrig.Mesh.Vertices.ToPoint3dArray();

            for (int i = 0; i < n; i++)
            {
                var f = cMesh.Mesh.Faces[faces[i]];
                var F = cMeshOrig.Mesh.Faces[Faces[i]];

                Point3d x1I, x2I, x3I;

                x1I = pts[f.A];
                x2I = pts[f.B];
                x3I = pts[f.C];

                Point3d X1I, X2I, X3I;

                X1I = Pts[F.A];
                X2I = Pts[F.B];
                X3I = Pts[F.C];

                Vector3d V1I = X2I - X1I;
                Vector3d V2I = X3I - X1I;

                Vector3d v1I = x2I - x1I;
                Vector3d v2I = x3I - x1I;

                var AI = ComputeA(v1I, v2I, V1I, V2I);

                double aI, bI, cI, dI;

                aI = AI[0, 0];
                bI = AI[0, 1];
                cI = AI[1, 0];
                dI = AI[1, 1];

                double phiI = ComputePhi(aI, bI, cI, dI);
                double psiI = ComputePsi(aI, bI, cI, dI);
               
                var sigma = ComputeSigma(aI, bI, cI, dI);

                var ang = (psiI - goalAngle[i]) % Math.PI;




                //error[i] = strength * (sigma[0] - sigma[1]) * Math.Tan(0.5 * (phiI - psiI - goalAngle[i]));
                //error[i] = strength * (sigma[0] - sigma[1]) * Math.Tan(0.5 * (psiI - goalAngle[i]));
                error[i] = strength * Math.Tan(0.5 * (ang));
                //error[i] = strength * Math.Tan(0.5 * (ph iI - psiI - goalAngle[i]));
                //error[i] = strength *( psiI - goalAngle[i]); 
            }
            return error;
        }

        public override SparseMatrixBuilder Jacobian(CMesh cMesh)
        {
            int n = cMesh.Mesh.Faces.Count;
            int rows = n;
            int columns = cMesh.DOF;
            var pts = cMesh.Mesh.Vertices.ToPoint3dArray();
            var Pts = cMeshOrig.Mesh.Vertices.ToPoint3dArray();

            List<Tuple<int, int, double>> elems = new List<Tuple<int, int, double>>();

            for (int i = 0; i < n; i++)
            {
                var f = cMesh.Mesh.Faces[faces[i]];
                var F = cMeshOrig.Mesh.Faces[Faces[i]];

                Point3d x1I, x2I, x3I;

                x1I = pts[f.A];
                x2I = pts[f.B];
                x3I = pts[f.C];

                Point3d X1I, X2I, X3I;

                X1I = Pts[F.A];
                X2I = Pts[F.B];
                X3I = Pts[F.C];

                Vector3d V1I = X2I - X1I;
                Vector3d V2I = X3I - X1I;

                Vector3d v1I = x2I - x1I;
                Vector3d v2I = x3I - x1I;

                var AI = ComputeA(v1I, v2I, V1I, V2I);

                double aI, bI, cI, dI;

                aI = AI[0, 0];
                bI = AI[0, 1];
                cI = AI[1, 0];
                dI = AI[1, 1];


                var UI = ComputeU(V1I, V2I);
                var sigmaI = ComputeSigma(aI, bI, cI, dI);
                var phiI = ComputePhi(aI, bI, cI, dI);
                var psiI = ComputePsi(aI, bI, cI, dI);
                var cos = Math.Cos(0.5*(psiI - goalAngle[i]));


                var dphiDAI = ComputeDphiDA(aI, bI, cI, dI);
                var dpsiDAI = ComputeDpsiDA(aI, bI, cI, dI);
                var dAIDuI = ComputeDADu(UI);
                var duIDvI = ComputeDuDv(v1I, v2I);
                var dvIDxI = ComputeDvDxI();
                var dsigmaDAI = ComputeDSigmaDA(AI);
                var dPhiDsigmaDAI = Matrix<double>.Build.Dense(1, 2);
                dPhiDsigmaDAI[0, 0] = Math.Tan(0.5 * (phiI - psiI - goalAngle[i]));
                dPhiDsigmaDAI[0, 1] = -Math.Tan(0.5 * (phiI - psiI - goalAngle[i]));
                var dPhiDphipsi = 0.5 * (sigmaI[0] - sigmaI[1]) / cos / cos;
                dPhiDphipsi = 0.5 / cos / cos;





                //var dPhiDx = dPhiDphipsi * (dpsiDAI) * dAIDuI * duIDvI * dvIDxI
                  //  + dPhiDsigmaDAI * dsigmaDAI * dAIDuI * duIDvI * dvIDxI;
                //var dPhiDx = dPhiDphipsi * (dpsiDAI) * dAIDuI * duIDvI * dvIDxI;
                var dPhiDx = dPhiDphipsi * (dpsiDAI) * dAIDuI * duIDvI * dvIDxI;
                dPhiDx *= strength;

                elems.Add(new Tuple<int, int, double>(i, 3 * f.A, dPhiDx[0, 0]));
                elems.Add(new Tuple<int, int, double>(i, 3 * f.A + 1, dPhiDx[0, 1]));
                elems.Add(new Tuple<int, int, double>(i, 3 * f.A + 2, dPhiDx[0, 2]));

                elems.Add(new Tuple<int, int, double>(i, 3 * f.B, dPhiDx[0, 3]));
                elems.Add(new Tuple<int, int, double>(i, 3 * f.B + 1, dPhiDx[0, 4]));
                elems.Add(new Tuple<int, int, double>(i, 3 * f.B + 2, dPhiDx[0, 5]));

                elems.Add(new Tuple<int, int, double>(i, 3 * f.C, dPhiDx[0, 6]));
                elems.Add(new Tuple<int, int, double>(i, 3 * f.C + 1, dPhiDx[0, 7]));
                elems.Add(new Tuple<int, int, double>(i, 3 * f.C + 2, dPhiDx[0, 8]));
            }
            return new SparseMatrixBuilder(rows, columns, elems);
        }
        private Matrix<double> ComputeDSigmaDA(Matrix<double> A)
        {
            var svd = A.Svd();
            var U = svd.U.ToColumnArrays();
            var V = svd.VT.ToRowArrays();
            var u1 = Vector<double>.Build.DenseOfArray(U[0]);
            var u2 = Vector<double>.Build.DenseOfArray(U[1]);
            var v1 = Vector<double>.Build.DenseOfArray(V[0]);
            var v2 = Vector<double>.Build.DenseOfArray(V[1]);
            var u1v1 = u1.OuterProduct(v1);
            var u2v2 = u2.OuterProduct(v2);
            Matrix<double> dSigmaDA = Matrix<double>.Build.Dense(2, 4);

            dSigmaDA[0, 0] = u1v1[0,0];
            dSigmaDA[0, 1] = u1v1[1,0];
            dSigmaDA[0, 2] = u1v1[0,1];
            dSigmaDA[0, 3] = u1v1[1,1];
            dSigmaDA[1, 0] = u2v2[0,0];
            dSigmaDA[1, 1] = u2v2[1,0];
            dSigmaDA[1, 2] = u2v2[0,1];
            dSigmaDA[1, 3] = u2v2[1,1];
            return dSigmaDA;
        }

        private Matrix<double> ComputeDphiDA(double a, double b, double c, double d)
        {
            Matrix<double> dphiDA = Matrix<double>.Build.Dense(1, 4);
            double den = Math.Pow(a + d, 2) + Math.Pow(c - b, 2) + 1e-12;
            dphiDA[0, 0] = -(c - b) / den;
            dphiDA[0, 1] = (a + d) / den;
            dphiDA[0, 2] = -(a + d) / den;
            dphiDA[0, 3] = -(c - b) / den;
            return dphiDA;
        }
        private Matrix<double> ComputeDpsiDA(double a, double b, double c, double d)
        {
            Matrix<double> dpsiDA = Matrix<double>.Build.Dense(1, 4);
            double A = a * a + c * c - b * b - d * d;
            double B = 2 * (a * b + c * d);
            double C = A * A;
            double D = B * B;
            dpsiDA[0, 0] = (A * b - B * a) / (C + D + 1e-12);
            dpsiDA[0, 1] = (A * d - B * c) / (C + D + 1e-12);
            dpsiDA[0, 2] = (A * a + B * b) / (C + D + 1e-12);
            dpsiDA[0, 3] = (A * c + B * d) / (C + D + 1e-12);
            return dpsiDA;
        }
        private Matrix<double> ComputeDthetaDv(Vector3d v1, Vector3d v2)
        {
            double alpha = (v1 * v2) / v1.Length / v2.Length;
            Matrix<double> dthetaDv = Matrix<double>.Build.Dense(1, 6);
            Vector3d dthetaDv1 = -(v2 / v1.Length / v2.Length - v1 * alpha / v1.Length / v1.Length) / (Math.Sqrt(1 - alpha * alpha) + 1e-12);
            Vector3d dthetaDv2 = -(v1 / v1.Length / v2.Length - v2 * alpha / v2.Length / v2.Length) / (Math.Sqrt(1 - alpha * alpha) + 1e-12);
            dthetaDv[0, 0] = dthetaDv1.X;
            dthetaDv[0, 1] = dthetaDv1.Y;
            dthetaDv[0, 2] = dthetaDv1.Z;
            dthetaDv[0, 3] = dthetaDv2.X;
            dthetaDv[0, 4] = dthetaDv2.Y;
            dthetaDv[0, 5] = dthetaDv2.Z;
            return dthetaDv;
        }
        private Matrix<double> ComputeDADu(Matrix<double> U)
        {
            Matrix<double> UInverseTranspose = (U.Inverse()).Transpose();
            Matrix<double> res = Matrix<double>.Build.Dense(4, 4);
            res[0, 0] = UInverseTranspose[0, 0];
            res[1, 1] = UInverseTranspose[0, 0];
            res[0, 2] = UInverseTranspose[0, 1];
            res[1, 3] = UInverseTranspose[0, 1];
            res[2, 0] = UInverseTranspose[1, 0];
            res[3, 1] = UInverseTranspose[1, 0];
            res[2, 2] = UInverseTranspose[1, 1];
            res[3, 3] = UInverseTranspose[1, 1];
            return res;
        }

        private Matrix<double> ComputeDuDv(Vector3d v1, Vector3d v2)
        {
            Vector3d du00dv1 = v1/v1.Length;
            Vector3d du00dv2 = Vector3d.Zero;
            Vector3d du01dv1 = v2 / v1.Length - (v1 * v2) / Math.Pow(v1.Length, 3) * v1;
            Vector3d du01dv2 = v1/v1.Length;
            Vector3d du10dv1 = Vector3d.Zero;
            Vector3d du10dv2 = Vector3d.Zero;
            Vector3d du11dv1
                = (v2.SquareLength * v1 - (v1 * v2) * v2) / v1.Length / (Vector3d.CrossProduct(v1, v2).Length + 1e-12)
                - Vector3d.CrossProduct(v1, v2).Length / Math.Pow(v1.Length, 3) * v1;
            Vector3d du11dv2
                = (v1.SquareLength * v2 - (v1 * v2) * v1) / v1.Length / (Vector3d.CrossProduct(v1, v2).Length + 1e-12);
            Matrix<double> duDv = Matrix<double>.Build.Dense(4, 6);
            duDv[0, 0] = du00dv1.X;
            duDv[0, 1] = du00dv1.Y;
            duDv[0, 2] = du00dv1.Z;
            duDv[0, 3] = du00dv2.X;
            duDv[0, 4] = du00dv2.Y;
            duDv[0, 5] = du00dv2.Z;
            duDv[1, 0] = du10dv1.X;
            duDv[1, 1] = du10dv1.Y;
            duDv[1, 2] = du10dv1.Z;
            duDv[1, 3] = du10dv2.X;
            duDv[1, 4] = du10dv2.Y;
            duDv[1, 5] = du10dv2.Z;
            duDv[2, 0] = du01dv1.X;
            duDv[2, 1] = du01dv1.Y;
            duDv[2, 2] = du01dv1.Z;
            duDv[2, 3] = du01dv2.X;
            duDv[2, 4] = du01dv2.Y;
            duDv[2, 5] = du01dv2.Z;
            duDv[3, 0] = du11dv1.X;
            duDv[3, 1] = du11dv1.Y;
            duDv[3, 2] = du11dv1.Z;
            duDv[3, 3] = du11dv2.X;
            duDv[3, 4] = du11dv2.Y;
            duDv[3, 5] = du11dv2.Z;
            return duDv;
        }

        private Matrix<double> ComputeDvDxI()
        {
            Matrix<double> dvDx = Matrix<double>.Build.Dense(6, 9);
            dvDx[0, 0] = -1;
            dvDx[1, 1] = -1;
            dvDx[2, 2] = -1;
            dvDx[3, 0] = -1;
            dvDx[4, 1] = -1;
            dvDx[5, 2] = -1;
            dvDx[0, 3] = 1;
            dvDx[1, 4] = 1;
            dvDx[2, 5] = 1;
            dvDx[3, 6] = 1;
            dvDx[4, 7] = 1;
            dvDx[5, 8] = 1;
            return dvDx;
        }

        private double[] ComputeSigma(double a, double b, double c, double d)
        {
            double alpha = a * a + c * c;
            double gamma = b * b + d * d;
            double beta = a * b + c * d;
            double A = Math.Pow(alpha - gamma, 2) + 4 * beta * beta;
            double sigma1 = Math.Sqrt((alpha + gamma + Math.Sqrt(A)) / 2);
            double sigma2 = Math.Sqrt((alpha + gamma - Math.Sqrt(A)) / 2);
            return new double[] { sigma1, sigma2 };
        }

        private double ComputeTanPhi(double a, double b, double c, double d)
        {
            return (c - b) / (a + b);
        }

        private double ComputeTanPsi(double a, double b, double c, double d)
        {
            return 2 * (a * b + c * d) / ((a * a + c * c) - (b * b + d * d));
        }

        private double ComputePhi(double a, double b, double c, double d)
        {
            return Math.Atan2(c - b, a + d);
        }
        private double ComputePsi(double a, double b, double c, double d)
        {
            return 0.5 * Math.Atan2(2 * (a * b + c * d), (a * a + c * c) - (b * b + d * d));
        }
        private double ComputeTheta(Vector3d v1, Vector3d v2)
        {
            return Math.Acos((v1 * v2) / v1.Length / v2.Length);
        }
        private Matrix<double> ComputeU(Vector3d v1, Vector3d v2)
        {
            Matrix<double> u = Matrix<double>.Build.Dense(2, 2);
            u[0, 0] = v1.Length;
            u[0, 1] = (v1*v2)/v1.Length;
            u[1, 1] = Vector3d.CrossProduct(v1,v2).Length/v1.Length;
            return u;
        }

        private Matrix<double> ComputeA(Vector3d v1, Vector3d v2, Vector3d V1, Vector3d V2)
        {
            var u = ComputeU(v1, v2);
            var U = ComputeU(V1, V2);
            return u * U.Inverse();
        }
        private Matrix<double> ComputeDSigmaIDx(Vector3d v1, Vector3d v2, Vector3d V1, Vector3d V2)
        {
            var A = ComputeA(v1, v2, V1, V2);
            var U = ComputeU(V1, V2);
            var sigma = ComputeSigma(A[0, 0], A[0, 1], A[1, 0], A[1, 1]);
            //var dSigmaDA = ComputeDSigmaDA(sigma[0], sigma[1], A[0, 0], A[0, 1], A[1, 0], A[1, 1]);
            var dSigmaDA = ComputeDSigmaDA(A);
            var dADu = ComputeDADu(U);
            var duDv = ComputeDuDv(v1, v2);
            var dvDx = ComputeDvDxI();
            return dSigmaDA * dADu * duDv * dvDx;
        }


    }
}
