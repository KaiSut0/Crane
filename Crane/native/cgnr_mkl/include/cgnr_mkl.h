#pragma once
#ifdef CGNRMKL_EXPORTS
#  define DLL_API __declspec(dllexport)
#else
#  define DLL_API __declspec(dllimport)
#endif

/* CGNR ─ 最小二乗 (長方形可) */
extern "C" DLL_API
int cgnr_solve_csr_double(
    int m,int n,
    const int* Ap,const int* Aj,const double* Ax,
    const double* b,double* x,int maxIter,double tol);

/* CG ─ 対称正定値 (n×n) */
extern "C" DLL_API
int cg_solve_csr_double(
    int n,
    const int* Ap,const int* Aj,const double* Ax,
    const double* b,double* x,int maxIter,double tol);