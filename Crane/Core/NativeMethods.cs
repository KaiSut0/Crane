using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Concurrent;
using System.Reflection;
using System.Runtime.InteropServices;
using System.IO;
using Crane.Components.Solver;

namespace Crane.Core
{
    internal static class NativeResolver
    {
        private static readonly (string logical, string win64, string macArm64, string other)[] Map =
        {
            ("cgnr", "cgnr.dll", "libcgnr.dylib", null),
            //("gram", "gram.dll", "libgram.dylib", null),
            ("gram", "gram.dll", null, null)
        };


        private static readonly ConcurrentDictionary<string, IntPtr> Cache = new();
        static NativeResolver()  // ← アセンブリ読み込み時に 1 回だけ呼ばれる
        {
            NativeLibrary.SetDllImportResolver(
                Assembly.GetExecutingAssembly(), Resolve);
        }
        private static IntPtr Resolve(string name, Assembly asm, DllImportSearchPath? _)
        {
            var craneAsm = typeof(CraneStaticSolver).Assembly;
            var baseDir =  Path.GetDirectoryName(craneAsm.Location);

            if (Cache.TryGetValue(name, out var h))               // すでにロード済み
                return h;

            foreach (var (logical, win64, macArm64, other) in Map)
                if (name == logical)
                {
                    string file = (OperatingSystem.IsMacOS() && RuntimeInformation.ProcessArchitecture==Architecture.Arm64) ? macArm64  :
                                  (OperatingSystem.IsWindows() && RuntimeInformation.ProcessArchitecture==Architecture.X64) ? win64  : other;
                    string full = Path.Combine(baseDir, file);
                    if (NativeLibrary.TryLoad(full, asm, _, out h))
                        return Cache[name] = h;
                }
            return IntPtr.Zero;                                   // 既定の検索に委ねる
        }

    }
    internal static class NativeMethods
    {

        static NativeMethods()
        {
            _ = typeof(NativeResolver);
        }

        [DllImport("cgnr", EntryPoint = "CGNRForRect", CallingConvention = CallingConvention.Cdecl)]
        internal extern static void CGNRForRect(int n, int m, [In] int[] csrRowPtr, [In] int[] csrColInd,
            [In] double[] csrVal, [In] double[] b, [In, Out] double[] x, double threshold, int iterationMax);
        [DllImport("cgnr", EntryPoint = "CGNRForSym", CallingConvention = CallingConvention.Cdecl)]
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

        [DllImport("gram",
            EntryPoint = "gram_mkl_build_lp64",
            CallingConvention = CallingConvention.Cdecl)]
        internal static extern int BuildMkl(
            int mA, int n,
            int[] Ap, int[] Aj, double[] Ax,
            int mB,
            int[] Bp, int[] Bj, double[] Bx,
            double w,
            out IntPtr Cp, out IntPtr Cj, out IntPtr Cv);

        [DllImport("gram",                     // libgram25.dylib / .so
            EntryPoint = "gram25_build_lp64",
            CallingConvention = CallingConvention.Cdecl)]
        internal static extern int Build(
            int mA, int n,
            int[] Ap, int[] Ac, double[] Av,
            int mB,
            int[] Bp, int[] Bc, double[] Bv,
            double w,
            out IntPtr Cptr, out IntPtr Ccol, out IntPtr Cval);

    }
}
