using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;

namespace Crane.Core
{
    internal static class NativeMethods
    {
        const string DLLName = "cgnr.dll";
        [DllImport(DLLName, EntryPoint = "CGNRForRect", CallingConvention = CallingConvention.Cdecl)]
        internal extern static void CGNRForRect(int n, int m, [In] int[] csrRowPtr, [In] int[] csrColInd,
            [In] double[] csrVal, [In] double[] b, [In, Out] double[] x, double threshold, int iterationMax);
        [DllImport(DLLName, EntryPoint = "CGNRForSym", CallingConvention = CallingConvention.Cdecl)]
        internal extern static void CGNRForSym(int n, [In] int[] csrRowPtr, [In] int[] csrColInd,
            [In] double[] csrVal, [In] double[] b, [In, Out] double[] x, double threshold, int iterationMax);

        [DllImport("cgnr", EntryPoint = "cgnr_solve_lp64", CallingConvention = CallingConvention.Cdecl)]
        internal extern static void CGNRSolve_macOS(
            int n, int m,
            int[] rowptr,
            int[] colind,
            double[] vals,
            double[] b,
            [In, Out] double[] x,
            double tol,
            int maxit);
        [DllImport("cgnr", EntryPoint = "cg_solve_lp64", CallingConvention = CallingConvention.Cdecl)]
        internal static extern int CgSolve(
            int n,
            int[] rowptr, int[] col, double[] vals,
            double[] b,
            [In, Out] double[] x,
            double tol, int maxit);


        static NativeMethods()
        {
            NativeLibrary.SetDllImportResolver(
                typeof(NativeMethods).Assembly,
                (name, asm, path) =>
                {
                    if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                    {
                        if(RuntimeInformation.ProcessArchitecture == Architecture.X64)
                        {
                            return NativeLibrary.Load("cgnr.dll", asm, path);
                        }
                        else
                        {
                            return IntPtr.Zero;
                        }
                    }
                    else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
                    {
                        if(RuntimeInformation.ProcessArchitecture == Architecture.Arm64)
                        {
                            return NativeLibrary.Load("cgnr", asm, path);
                        }
                        else
                        {
                            return IntPtr.Zero;
                        }
                    }
                    else
                    {
                        return IntPtr.Zero;
                    }
                }
                );
        }
    }
}
