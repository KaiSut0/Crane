#pragma once
#include <stddef.h>   // size_t
#include <cstdlib>

#ifdef GRAMMKL_EXPORTS        // DLL 側で定義
#  define DLL_API __declspec(dllexport)
#else
#  define DLL_API __declspec(dllimport)
#endif

extern "C"
{
DLL_API int gram_mkl_build_lp64(
    int mA, int n, const int *Ap, const int *Aj, const double *Ax,
    int mB, const int *Bp, const int *Bj, const double *Bx,
    double w,
    int **Cp, int **Cj, double **Cx
);
}
