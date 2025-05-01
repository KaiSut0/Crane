using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.LinearAlgebra.Double;
using MathNet.Numerics.LinearAlgebra.Storage;
using System.Runtime.InteropServices;

namespace Crane.Core
{
    internal static class LinearAlgebra
    {
        internal static Vector<double> Solve(SparseMatrix A, Vector<double> b, Vector<double> x, double threshold, int iterationMax)
        {
            var cpuArchitecture = RuntimeInformation.ProcessArchitecture;
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                if (cpuArchitecture == Architecture.X64)
                {
                    return SolveMKL(A, b, x, threshold, iterationMax);
                }
                else
                {
                    return SolveManaged(A, b, x, threshold, iterationMax);
                }
            }
            else
            {
                return SolveManaged(A, b, x, threshold, iterationMax);
            }
        }
        private static Vector<double> SolveManaged(SparseMatrix A, Vector<double> b, Vector<double> x, double threshold, int iterationMax)
        {
            int iteration = 0;
            Matrix<double> AT = A.Transpose();
            Vector<double> r = b - A * x;
            Vector<double> p = AT * r;
            double alpha, beta;
            int matSize = Math.Min(A.ColumnCount, A.RowCount);

            while ((iteration < Math.Min(iterationMax, matSize + 1)) && r.L2Norm() > threshold)
            {
                Vector<double> ATr0 = AT * r;
                Vector<double> Ap = A * p;
                alpha = Math.Pow(ATr0.L2Norm(), 2) / Math.Pow(Ap.L2Norm(), 2);
                x += alpha * p;
                r = b - A * x;
                Vector<double> ATr1 = AT * r;
                beta = Math.Pow(ATr1.L2Norm(), 2) / Math.Pow(ATr0.L2Norm(), 2);
                p = ATr1 + beta * p;
                iteration++;
            }
            return x;
        }
        private static Vector<double> SolveMKL(SparseMatrix A, Vector<double> b, Vector<double> x, double threshold, int iterationMax)
        {

            SparseCompressedRowMatrixStorage<double> storage =
            (SparseCompressedRowMatrixStorage<double>)A.Storage;
            int n = storage.RowCount;
            int m = storage.ColumnCount;
            int[] csrRowPtr = storage.RowPointers;
            int[] csrColInd = storage.ColumnIndices;
            double[] csrVal = storage.Values;
            double[] answer = x.ToArray();
            NativeMethods.CGNRForRect(n, m, csrRowPtr, csrColInd, csrVal, b.ToArray(), answer, threshold, iterationMax);

            return Vector<double>.Build.DenseOfArray(answer);
        }
    
        internal static Vector<double> SolveSym(SparseMatrix A, Vector<double> b, double threshold, int iterationMax)
        {
            var cpuArchitecture = RuntimeInformation.ProcessArchitecture;
            if(cpuArchitecture == Architecture.X64)
            {
                return SolveSymMKL(A, b, threshold, iterationMax);
            }
            else
            {
                return SolveSymManaged(A, b, threshold, iterationMax);
            }
        }
        private static Vector<double> SolveSymMKL(SparseMatrix A, Vector<double> b, double threshold, int iterationMax)
        {
            SparseCompressedRowMatrixStorage<double> storage =
                (SparseCompressedRowMatrixStorage<double>)A.Storage;
            int n = storage.RowCount;
            int[] csrRowPtr = storage.RowPointers;
            int[] csrColInd = storage.ColumnIndices;
            double[] csrVal = storage.Values;
            double[] answer = new double[n];
            NativeMethods.CGNRForSym(n, csrRowPtr, csrColInd, csrVal, b.ToArray(), answer, threshold, iterationMax);
            return Vector<double>.Build.DenseOfArray(answer);
        }
        private static Vector<double> SolveSymManaged(SparseMatrix A, Vector<double> b, double threshold, int iterationMax)
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
    }
}
