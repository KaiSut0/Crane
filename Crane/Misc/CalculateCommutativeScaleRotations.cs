using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.LinearAlgebra.Double;

namespace Crane.Misc
{
    public class CalculateCommutativeScaleRotations
    {
        public double s1 { get; private set; }
        public double s2 { get; private set; }
        public double t1 { get; private set; }
        public double t2 { get; private set; }
        public double[] n { get; private set; }
        public double[] r { get; private set; }
        public double[] X00 { get; }
        public double[] X01 { get; }
        public double[] X02 { get; }
        public double[] X10 { get; }

        private Vector<double> error()
        {
            Vector<double> x00 = Vector<double>.Build.DenseOfArray(new double[] {X00[0], X00[1], X00[2], 1});
            Vector<double> x01 = Vector<double>.Build.DenseOfArray(new double[] {X01[0], X01[1], X01[2], 1});
            Vector<double> x02 = Vector<double>.Build.DenseOfArray(new double[] {X02[0], X02[1], X02[2], 1});
            Vector<double> x10 = Vector<double>.Build.DenseOfArray(new double[] {X10[0], X10[1], X10[2], 1});

            Matrix<double> R = Matrix<double>.Build.Dense(4, 4);
            R[0, 0] = R[1, 1] = R[2, 2] = R[3, 3] = 1.0;
            Matrix<double> Rinv = R.Clone();
            R[3, 0] = r[0];
            R[3, 1] = r[1];
            R[3, 2] = r[2];
            Rinv[3, 0] = -r[0];
            Rinv[3, 1] = -r[1];
            Rinv[3, 2] = -r[2];

            Matrix<double> N = Matrix<double>.Build.Dense(3, 3);
            N[0, 1] = -n[2];
            N[0, 2] = n[1];
            N[1, 0] = n[2];
            N[1, 2] = -n[0];
            N[2, 0] = -n[1];
            N[2, 1] = n[0];
            Matrix<double> I = Matrix<double>.Build.DiagonalIdentity(3);
            Matrix<double> s1R1 = s1*(I + Math.Sin(t1) * N + (1 - Math.Cos(t1)) * N * N);
            Matrix<double> s2R2 = s2*(I + Math.Sin(t2) * N + (1 - Math.Cos(t2)) * N * N);
            var s1R1_ = Matrix<double>.Build.Dense(4, 4);
            var s2R2_ = Matrix<double>.Build.Dense(4, 4);
            s1R1_.SetSubMatrix(0, 0, s1R1);
            s2R2_.SetSubMatrix(0, 0, s2R2);
            s1R1_[3, 3] = 1;
            s2R2_[3, 3] = 1;
            Matrix<double> R1 = R * s1R1_ * Rinv;
            Matrix<double> R2 = R * s2R2_ * Rinv;

            var obj1 = x01 - R1 * x00;
            var obj2 = x02 - R1 * x01;
            var obj3 = x10 - R2 * x00;
            var obj4 = n[0] * n[0] + n[1] * n[1] + n[2] * n[2] - 1;

            Vector<double> error = Vector<double>.Build.DenseOfArray(new double[]
            {
                obj1[0], obj1[1], obj1[2],
                obj2[0], obj2[1], obj2[2],
                obj3[0], obj3[1], obj3[2],
                obj4
            });
            return error;
        }

        private Matrix<double> jacobian()
        {
            Vector<double> x00 = Vector<double>.Build.DenseOfArray(new double[] {X00[0], X00[1], X00[2], 1});
            Vector<double> x01 = Vector<double>.Build.DenseOfArray(new double[] {X01[0], X01[1], X01[2], 1});
            Vector<double> x02 = Vector<double>.Build.DenseOfArray(new double[] {X02[0], X02[1], X02[2], 1});
            Vector<double> x10 = Vector<double>.Build.DenseOfArray(new double[] {X10[0], X10[1], X10[2], 1});

            Matrix<double> R = Matrix<double>.Build.Dense(4, 4);
            R[0, 0] = R[1, 1] = R[2, 2] = R[3, 3] = 1.0;
            Matrix<double> Rinv = R.Clone();
            R[3, 0] = r[0];
            R[3, 1] = r[1];
            R[3, 2] = r[2];
            Rinv[3, 0] = -r[0];
            Rinv[3, 1] = -r[1];
            Rinv[3, 2] = -r[2];

            Matrix<double> N = Matrix<double>.Build.Dense(3, 3);
            N[0, 1] = -n[2];
            N[0, 2] = n[1];
            N[1, 0] = n[2];
            N[1, 2] = -n[0];
            N[2, 0] = -n[1];
            N[2, 1] = n[0];
            Matrix<double> I = Matrix<double>.Build.DiagonalIdentity(3);
            Matrix<double> s1R1 = s1*(I + Math.Sin(t1) * N + (1 - Math.Cos(t1)) * N * N);
            Matrix<double> s2R2 = s2*(I + Math.Sin(t2) * N + (1 - Math.Cos(t2)) * N * N);
            var s1R1_ = Matrix<double>.Build.Dense(4, 4);
            var s2R2_ = Matrix<double>.Build.Dense(4, 4);
            s1R1_.SetSubMatrix(0, 0, s1R1);
            s2R2_.SetSubMatrix(0, 0, s2R2);
            s1R1_[3, 3] = 1;
            s2R2_[3, 3] = 1;
            Matrix<double> R1 = R * s1R1_ * Rinv;
            Matrix<double> R2 = R * s2R2_ * Rinv;

            var obj1 = x01 - R1 * x00;
            var obj2 = x02 - R1 * x01;
            var obj3 = x10 - R2 * x00;
            var obj4 = n[0] * n[0] + n[1] * n[1] + n[2] * n[2] - 1;

            Vector<double> error = Vector<double>.Build.DenseOfArray(new double[]
            {
                obj1[0], obj1[1], obj1[2],
                obj2[0], obj2[1], obj2[2],
                obj3[0], obj3[1], obj3[2],
                obj4
            });


            var dNdnx = Matrix<double>.Build.Dense(3, 3);
            var dNdny = Matrix<double>.Build.Dense(3, 3);
            var dNdnz = Matrix<double>.Build.Dense(3, 3);
            var dRdt1 = Matrix<double>.Build.Dense(3, 3);
            var dRdt2 = Matrix<double>.Build.Dense(3, 3);
            dNdnx[1, 2] = -1;
            dNdnx[2, 1] = 1;
            dNdny[0, 2] = 1;
            dNdny[2, 0] = -1;
            dNdnz[0, 1] = -1;
            dNdnz[1, 0] = 1;
            dRdt1 = Math.Cos(t1) * N + Math.Sin(t1) * N * N;
            dRdt2 = Math.Cos(t2) * N + Math.Sin(t2) * N * N;
            var dR1dnx = Math.Sin(t1) * dNdnx - Math.Cos(t1) * (dNdnx * N + N * dNdnx);
            var dR1dny = Math.Sin(t1) * dNdny - Math.Cos(t1) * (dNdny * N + N * dNdny);
            var dR1dnz = Math.Sin(t1) * dNdnz - Math.Cos(t1) * (dNdnz * N + N * dNdnz);
            var dR2dnx = Math.Sin(t2) * dNdnx - Math.Cos(t2) * (dNdnx * N + N * dNdnx);
            var dR2dny = Math.Sin(t2) * dNdny - Math.Cos(t2) * (dNdny * N + N * dNdny);
            var dR2dnz = Math.Sin(t2) * dNdnz - Math.Cos(t2) * (dNdnz * N + N * dNdnz);

            var dRdrx = Matrix<double>.Build.Dense(4, 4);
            var dRdry = Matrix<double>.Build.Dense(4, 4);
            var dRdrz = Matrix<double>.Build.Dense(4, 4);
            dRdrx[3, 0] = 1;
            dRdry[3, 1] = 1;
            dRdrz[3, 2] = 1;

            throw new NotImplementedException();

        }


    }
}
