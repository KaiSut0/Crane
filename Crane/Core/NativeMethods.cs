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
       
    }
}
