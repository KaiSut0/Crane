using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Crane.Core;
using MathNet.Numerics.LinearAlgebra;
using QuickGraph.Serialization;
using Rhino.Geometry;

namespace Crane.Constraints
{
    public class FixSingularValue : Constraint
    {
        public FixSingularValue(CMesh cMeshOrig, double[] goalSigma1, double[] goalSigma2)
        {
            this.cMeshOrig = new CMesh(cMeshOrig);
            this.goalSigma1 = goalSigma1;
            this.goalSigma2 = goalSigma2;
        }

        public FixSingularValue(CMesh cMesh, CMesh cMeshOrig, Point3d[] facePoints, Point3d[] FacePoints, double[] goalSigma1, double[] goalSigma2, int mode, double strength) 
        {
            this.cMeshOrig = new CMesh(cMeshOrig);
            
            this.faces = new int[facePoints.Length];
            this.Faces = new int[FacePoints.Length];
            for (int i = 0; i < facePoints.Length; i++)
            {
                faces[i] = cMesh.GetFaceIdFromFaceCenter(facePoints[i]);
                Faces[i] = cMeshOrig.GetFaceIdFromFaceCenter(FacePoints[i]);
            }

            this.mode = mode;
            this.goalSigma1 = goalSigma1;
            this.goalSigma2 = goalSigma2;
            this.strength = strength;
        }


        private int[] faces;
        private int[] Faces;
        private double strength;
        private double[] goalSigma1;
        private double[] goalSigma2;
        private CMesh cMeshOrig;
        private int mode;

        public override double[] Error(CMesh cMesh)
        {
            int n = faces.Length;
            double[] error = new double[n];
            if (mode == 0) error = new double[2 * n];
            var pts = cMesh.Mesh.Vertices.ToPoint3dArray();
            var Pts = cMeshOrig.Mesh.Vertices.ToPoint3dArray();
            for (int i = 0; i < n; i++)
            {
                var f = cMesh.Mesh.Faces[faces[i]];
                var x1 = pts[f.A];
                var x2 = pts[f.B];
                var x3 = pts[f.C];
                Vector3d v1 = x2 - x1;
                Vector3d v2 = x3 - x1;

                var F = cMeshOrig.Mesh.Faces[Faces[i]];
                var X1 = Pts[F.A];
                var X2 = Pts[F.B];
                var X3 = Pts[F.C];
                Vector3d V1 = X2 - X1;
                Vector3d V2 = X3 - X1;
                var A = ComputeA(v1, v2, V1, V2);
                var sigma = ComputeSigma(A[0, 0], A[0, 1], A[1, 0], A[1, 1]);
                if(mode == 0)
                {
                    error[2 * i] = sigma[0] - goalSigma1[i];
                    error[2 * i + 1] = sigma[1] - goalSigma2[i];
                }
                else if (mode == 1)
                {
                    error[i] = strength * (sigma[0] - goalSigma1[i]);
                }
                else if (mode == 2)
                {
                    error[i] = strength * (sigma[1] - goalSigma2[i]);
                }
            }
            return error;
        }

        public override SparseMatrixBuilder Jacobian(CMesh cMesh)
        {
            int n = cMesh.Mesh.Faces.Count;
            var pts = cMesh.Mesh.Vertices.ToPoint3dArray();
            var Pts = cMeshOrig.Mesh.Vertices.ToPoint3dArray();

            int rows = n;
            if (mode == 0) rows = 2 * n;
            int columns = cMesh.DOF;
            List<Tuple<int, int, double>> elems = new List<Tuple<int, int, double>>();

            for (int i = 0; i < n; i++)
            {
                var f = cMesh.Mesh.Faces[faces[i]];
                var x1 = pts[f.A];
                var x2 = pts[f.B];
                var x3 = pts[f.C];
                Vector3d v1 = x2 - x1;
                Vector3d v2 = x3 - x1;

                var F = cMeshOrig.Mesh.Faces[Faces[i]];
                var X1 = Pts[F.A];
                var X2 = Pts[F.B];
                var X3 = Pts[F.C];
                Vector3d V1 = X2 - X1;
                Vector3d V2 = X3 - X1;
                var dSigmaDx = strength * ComputeDSigmaDx(v1, v2, V1, V2);
                //var A = ComputeA(v1, v2, V1, V2);
                //var dSigmaDx = ComputeDSigmaDA(A);

                if (mode == 0)
                {
                    elems.Add(new Tuple<int, int, double>(2 * i, 3 * f.A, dSigmaDx[0, 0]));
                    elems.Add(new Tuple<int, int, double>(2 * i, 3 * f.A + 1, dSigmaDx[0, 1]));
                    elems.Add(new Tuple<int, int, double>(2 * i, 3 * f.A + 2, dSigmaDx[0, 2]));

                    elems.Add(new Tuple<int, int, double>(2 * i, 3 * f.B, dSigmaDx[0, 3]));
                    elems.Add(new Tuple<int, int, double>(2 * i, 3 * f.B + 1, dSigmaDx[0, 4]));
                    elems.Add(new Tuple<int, int, double>(2 * i, 3 * f.B + 2, dSigmaDx[0, 5]));

                    elems.Add(new Tuple<int, int, double>(2 * i, 3 * f.C, dSigmaDx[0, 6]));
                    elems.Add(new Tuple<int, int, double>(2 * i, 3 * f.C + 1, dSigmaDx[0, 7]));
                    elems.Add(new Tuple<int, int, double>(2 * i, 3 * f.C + 2, dSigmaDx[0, 8]));

                    elems.Add(new Tuple<int, int, double>(2 * i + 1, 3 * f.A, dSigmaDx[1, 0]));
                    elems.Add(new Tuple<int, int, double>(2 * i + 1, 3 * f.A + 1, dSigmaDx[1, 1]));
                    elems.Add(new Tuple<int, int, double>(2 * i + 1, 3 * f.A + 2, dSigmaDx[1, 2]));

                    elems.Add(new Tuple<int, int, double>(2 * i + 1, 3 * f.B, dSigmaDx[1, 3]));
                    elems.Add(new Tuple<int, int, double>(2 * i + 1, 3 * f.B + 1, dSigmaDx[1, 4]));
                    elems.Add(new Tuple<int, int, double>(2 * i + 1, 3 * f.B + 2, dSigmaDx[1, 5]));

                    elems.Add(new Tuple<int, int, double>(2 * i + 1, 3 * f.C, dSigmaDx[1, 6]));
                    elems.Add(new Tuple<int, int, double>(2 * i + 1, 3 * f.C + 1, dSigmaDx[1, 7]));
                    elems.Add(new Tuple<int, int, double>(2 * i + 1, 3 * f.C + 2, dSigmaDx[1, 8]));
                }

                if (mode == 1)
                {
                    elems.Add(new Tuple<int, int, double>(i, 3 * f.A, dSigmaDx[0, 0]));
                    elems.Add(new Tuple<int, int, double>(i, 3 * f.A + 1, dSigmaDx[0, 1]));
                    elems.Add(new Tuple<int, int, double>(i, 3 * f.A + 2, dSigmaDx[0, 2]));

                    elems.Add(new Tuple<int, int, double>(i, 3 * f.B, dSigmaDx[0, 3]));
                    elems.Add(new Tuple<int, int, double>(i, 3 * f.B + 1, dSigmaDx[0, 4]));
                    elems.Add(new Tuple<int, int, double>(i, 3 * f.B + 2, dSigmaDx[0, 5]));

                    elems.Add(new Tuple<int, int, double>(i, 3 * f.C, dSigmaDx[0, 6]));
                    elems.Add(new Tuple<int, int, double>(i, 3 * f.C + 1, dSigmaDx[0, 7]));
                    elems.Add(new Tuple<int, int, double>(i, 3 * f.C + 2, dSigmaDx[0, 8]));
                }

                if (mode == 2)
                {
                    elems.Add(new Tuple<int, int, double>(i, 3 * f.A, dSigmaDx[1, 0]));
                    elems.Add(new Tuple<int, int, double>(i, 3 * f.A + 1, dSigmaDx[1, 1]));
                    elems.Add(new Tuple<int, int, double>(i, 3 * f.A + 2, dSigmaDx[1, 2]));

                    elems.Add(new Tuple<int, int, double>(i, 3 * f.B, dSigmaDx[1, 3]));
                    elems.Add(new Tuple<int, int, double>(i, 3 * f.B + 1, dSigmaDx[1, 4]));
                    elems.Add(new Tuple<int, int, double>(i, 3 * f.B + 2, dSigmaDx[1, 5]));

                    elems.Add(new Tuple<int, int, double>(i, 3 * f.C, dSigmaDx[1, 6]));
                    elems.Add(new Tuple<int, int, double>(i, 3 * f.C + 1, dSigmaDx[1, 7]));
                    elems.Add(new Tuple<int, int, double>(i, 3 * f.C + 2, dSigmaDx[1, 8]));

                }

                //var svd = dSigmaDx.Svd();
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
        private Matrix<double> ComputeDSigmaDA(double sigma1, double sigma2, double a, double b, double c, double d)
        {
            Matrix<double> dSigmaDA = Matrix<double>.Build.Dense(2, 4);
            double alpha = a * a + c * c;
            double gamma = b * b + d * d;
            double beta = a * b + c * d;
            double delta = alpha - gamma;
            double G = delta * delta + 4 * beta * beta;
            double dSigma1Da = 1.0 / (2.0 * sigma1) * (a + (a * delta + 2 * b * beta) / (Math.Sqrt(G) + 1e-12));
            double dSigma1Db = 1.0 / (2.0 * sigma1) * (b + (-b * delta + 2 * a * beta) / (Math.Sqrt(G) + 1e-12));
            double dSigma1Dc = 1.0 / (2.0 * sigma1) * (c + (c * delta + 2 * d * beta) / (Math.Sqrt(G) + 1e-12));
            double dSigma1Dd = 1.0 / (2.0 * sigma1) * (d + (-d * delta + 2 * c * beta) / (Math.Sqrt(G) + 1e-12));
            double dSigma2Da = 1.0 / (2.0 * sigma2) * (a - (a * delta + 2 * b * beta) / (Math.Sqrt(G) + 1e-12));
            double dSigma2Db = 1.0 / (2.0 * sigma2) * (b - (-b * delta + 2 * a * beta) / (Math.Sqrt(G) + 1e-12));
            double dSigma2Dc = 1.0 / (2.0 * sigma2) * (c - (c * delta + 2 * d * beta) / (Math.Sqrt(G) + 1e-12));
            double dSigma2Dd = 1.0 / (2.0 * sigma2) * (d - (-d * delta + 2 * c * beta) / (Math.Sqrt(G) + 1e-12));

            dSigmaDA[0, 0] = dSigma1Da;
            dSigmaDA[0, 2] = dSigma1Db;
            dSigmaDA[0, 1] = dSigma1Dc;
            dSigmaDA[0, 3] = dSigma1Dd;
            dSigmaDA[1, 0] = dSigma2Da;
            dSigmaDA[1, 2] = dSigma2Db;
            dSigmaDA[1, 1] = dSigma2Dc;
            dSigmaDA[1, 3] = dSigma2Dd;
            return dSigmaDA;
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

        private Matrix<double> ComputeDvDx()
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

        private Matrix<double> ComputeDSigmaDx(Vector3d v1, Vector3d v2, Vector3d V1, Vector3d V2)
        {
            var A = ComputeA(v1, v2, V1, V2);
            var U = ComputeU(V1, V2);
            var sigma = ComputeSigma(A[0, 0], A[0, 1], A[1, 0], A[1, 1]);
            //var dSigmaDA = ComputeDSigmaDA(sigma[0], sigma[1], A[0, 0], A[0, 1], A[1, 0], A[1, 1]);
            var dSigmaDA = ComputeDSigmaDA(A);
            var dADu = ComputeDADu(U);
            var duDv = ComputeDuDv(v1, v2);
            var dvDx = ComputeDvDx();
            return dSigmaDA * dADu * duDv * dvDx;
        }

    }
}
