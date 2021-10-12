using System;
using System.Collections.Generic;
using System.Drawing.Text;
using Crane.Core;
using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.Statistics.Mcmc;
using Rhino.Geometry;

namespace Crane.Misc
{
    public class CalculateCommutativeTransform
    {
        public CalculateCommutativeTransform(double[,] l1ini, double[,] l2ini, double[] d1ini, double[] d2ini, bool detL1is1, bool detL2is1, double detL1Goal, double detL2Goal)
        {
            L1ini = copy(l1ini);
            L2ini = copy(l2ini);
            D1ini = copy(d1ini);
            D2ini = copy(d2ini);
            this.detL1is1 = detL1is1;
            this.detL2is1 = detL2is1;
            this.detL1Goal = detL1Goal;
            this.detL2Goal = detL2Goal;
        }
        public Transform A1
        {
            get
            {
                var a1 = new Transform();
                for (int i = 0; i < 3; i++)
                {
                    for (int j = 0; j < 3; j++)
                    {
                        a1[i, j] = L1[i, j];
                    }

                    a1[i, 3] = D1[i];
                }
                a1[3, 3] = 1;

                return a1;
            }
        }
        public Transform A2
        {
            get
            {
                var a2 = new Transform();
                for (int i = 0; i < 3; i++)
                {
                    for (int j = 0; j < 3; j++)
                    {
                        a2[i, j] = L2[i, j];
                    }

                    a2[i, 3] = D2[i];
                }
                a2[3, 3] = 1;

                return a2;
            }
        }
        public double[] Error
        {
            get
            {
                return error(L1, L2, D1, D2).ToArray();
            }
        }


        public double DetL1
        {
            get
            {
                return Matrix<double>.Build.DenseOfArray(L1).Determinant();
            }
        }
        public double DetL2
        {
            get
            {
                return Matrix<double>.Build.DenseOfArray(L2).Determinant();
            }
        }


        public void SolveCommutativeEquation(int iterationMax, double tolerance)
        {
            double[,] l1 = copy(L1ini);
            double[,] l2 = copy(L2ini);
            double[] d1 = copy(D1ini);
            double[] d2 = copy(D2ini);
            Vector<double> err = error(l1, l2, d1, d2);
            Matrix<double> jac = jacobian(l1, l2, d1, d2);
            Vector<double> dif = Vector<double>.Build.Dense(24);
            double residual = err.L2Norm();
            int iteration = 0;
            var speed = new List<double>();
            while (residual > tolerance && iteration < iterationMax)
            {
                dif = RigidOrigami.CGNRSolveForRectangleMatrix(jac, -err, Vector<double>.Build.Dense(24), 1e-12, 24, ref speed);
                update(dif, ref l1, ref l2, ref d1, ref d2);
                err = error(l1, l2, d1, d2);
                jac = jacobian(l1, l2, d1, d2);
                residual = err.L2Norm();
                iteration++;
            }
            L1 = l1;
            L2 = l2;
            D1 = d1;
            D2 = d2;
        }

        private double[,] L1;
        private double[,] L2;
        private double[] D1;
        private double[] D2;

        private double[,] L1ini;
        private double[,] L2ini;
        private double[] D1ini;
        private double[] D2ini;

        private bool detL1is1;
        private bool detL2is1;

        private double detL1Goal;
        private double detL2Goal;

        private Vector<double> error(double[,] l1, double[,] l2, double[] d1, double[] d2)
        {
            double[] err = new double[14];
            for (int i = 0; i < 3; i++)
            {
                for (int j = 0; j < 3; j++)
                {
                    for (int k = 0; k < 3; k++)
                    {
                        err[3 * i + j] += l1[i, k] * l2[k, j] - l2[i, k] * l1[k, j];
                    }

                    err[9 + i] += l1[i, j] * d2[j] - l2[i, j] * d1[j];
                }

                err[9 + i] += d1[i] - d2[i];
            }

            for (int i = 0; i < 3; i++)
            {
                for (int j = 0; j < 3; j++)
                {
                    for (int k = 0; k < 3; k++)
                    {
                        if (detL1is1)
                            err[12] += eps[i, j, k] * l1[0, i] * l1[1, j] * l1[2, k];
                        if (detL2is1)
                            err[13] += eps[i, j, k] * l2[0, i] * l2[1, j] * l2[2, k];
                    }
                }
            }

            if (detL1is1) err[12] -= detL1Goal;
            if (detL2is1) err[13] -= detL2Goal;

            return Vector<double>.Build.Dense(err);
        }

        private Matrix<double> jacobian(double[,] l1, double[,] l2, double[] d1, double[] d2)
        {
            double[,] jaco = new double[14, 24];
            for (int i = 0; i < 3; i++)
            {
                for (int j = 0; j < 3; j++)
                {
                    for (int p = 0; p < 2; p++)
                    {
                        for (int m = 0; m < 3; m++)
                        {
                            for (int n = 0; n < 3; n++)
                            {
                                for (int k = 0; k < 3; k++)
                                {
                                    jaco[3 * i + j, 3 * m + n + 9 * p] += del(0, p) * del(i, m) * del(k, n) * l2[k, j] + l1[i, k] * del(1, p) * del(k, m) * del(j, n)
                                                                          - del(1, p) * del(i, m) * del(k, n) * l1[k, j] - l2[i, k] * del(0, p) * del(k, m) * del(j, n);
                                }

                                jaco[9 + i, 3 * m + n + 9 * p] += del(0, p) * del(i, m) * del(j, n) * d2[j]
                                                                - del(1, p) * del(i, m) * del(j, n) * d1[j];

                            }
                            jaco[9 + i, 18 + m + 3 * p] += l1[i, j] * del(1, p) * del(j, m) + del(0, p) * del(i, m)
                                                         - l2[i, j] * del(0, p) * del(j, m) + del(1, p) * del(i, m);
                        }
                    }
                }
            }

            for (int i = 0; i < 3; i++)
            {
                for (int j = 0; j < 3; j++)
                {
                    for (int k = 0; k < 3; k++)
                    {
                        for (int m = 0; m < 3; m++)
                        {
                            for (int n = 0; n < 3; n++)
                            {
                                for (int p = 0; p < 2; p++)
                                {
                                    if (detL1is1)
                                        jaco[12, 3 * m + n + 9 * p] += eps[i, j, k] * del(p, 0) *
                                                                       (del(0, m) * del(i, n) * l1[1, j] * l1[2, k]
                                                                        + del(1, m) * del(j, n) * l1[0, i] * l1[2, k]
                                                                        + del(2, m) * del(k, n) * l1[0, i] * l1[1, j]);
                                    if (detL2is1)
                                        jaco[13, 3 * m + n + 9 * p] += eps[i, j, k] * del(p, 1) *
                                                                       (del(0, m) * del(i, n) * l2[1, j] * l2[2, k]
                                                                        + del(1, m) * del(j, n) * l2[0, i] * l2[2, k]
                                                                        + del(2, m) * del(k, n) * l2[0, i] * l2[1, j]);
                                }
                            }
                        }
                    }
                }
            }


            return Matrix<double>.Build.DenseOfArray(jaco);
        }

        private double del(int i, int j)
        {
            return i == j ? 1 : 0;
        }

        private static double[,,] eps = new double[,,]
        {
            { { 0, 0,  0 }, {  0, 0, 1 }, { 0, -1, 0 } },
            { { 0, 0, -1 }, {  0, 0, 0 }, { 1,  0, 0 } },
            { { 0, 1,  0 }, { -1, 0, 0 }, { 0,  0, 0 } }
        };

        private double[,] copy(double[,] array)
        {
            var copied = new double[3, 3];
            for (int i = 0; i < 3; i++)
                for (int j = 0; j < 3; j++)
                    copied[i, j] = array[i, j];
            return copied;
        }

        private double[] copy(double[] array)
        {
            var copied = new double[3];
            for (int i = 0; i < 3; i++)
                copied[i] = array[i];
            return copied;
        }

        private void update(Vector<double> diff, ref double[,] l1, ref double[,] l2, ref double[] d1, ref double[] d2)
        {
            for(int i = 0; i < 3; i++)
            {
                for(int j = 0; j < 3; j++)
                {
                    l1[i, j] += diff[3 * i + j];
                    l2[i, j] += diff[3 * i + j + 9];
                }

                d1[i] += diff[18 + i];
                d2[i] += diff[21 + i];
            }
        }
    }
}
