#include <math.h>
#include <stdlib.h>
#include <string.h>
#include "armpl.h"
#include "../include/cgnr_solver.h"

/* 内部ワーク確保 */
#define NEWVEC(ptr, n)               \
    do { (ptr) = calloc((n), sizeof(double)); if(!(ptr)) return -1; } while(0)

/* ArmPL スパース行列の生成 */
armpl_spmat_t
create_csr_d(int m, int n,
             const int* rowptr, const int* colind, const double* vals)
{
    armpl_spmat_t A;
    if (armpl_spmat_create_csr_d(&A, m, n,
            (const armpl_int_t*)rowptr,
            (const armpl_int_t*)colind,
            vals, 0) != ARMPL_STATUS_SUCCESS)
        return NULL;

    armpl_spmat_hint(A, ARMPL_SPARSE_HINT_SPMV_OPERATION,
                        ARMPL_SPARSE_OPERATION_NOTRANS);
    armpl_spmv_optimize(A);
    return A;
}

/* ----- メイン CGNR ------------------------------------------------- */
int cgnr_solve_lp64(int m, int n,
                    const int* rowptr, const int* colind, const double* val,
                    const double* b, double* x,
                    double tol, int maxit)
{
    armpl_spmat_t A = create_csr_d(m,n,rowptr,colind,val);
    if (!A) return -1;

    double *r, *p, *q, *At;
    NEWVEC(r,  m);
    NEWVEC(p,  n);
    NEWVEC(q,  m);
    NEWVEC(At, n);

    /* r0 = b - A·x0  (ここでは x0=0 を想定) */
    memcpy(r, b, m*sizeof(double));
    armpl_spmv_exec_d(ARMPL_SPARSE_OPERATION_NOTRANS, -1.0, A, x, 0.0, r);
    cblas_daxpy(m, 1.0, b, 1, r, 1);

    /* p0 = Aᵀ r0 */
    armpl_spmv_exec_d(ARMPL_SPARSE_OPERATION_TRANS, 1.0, A, r, 0.0, p);
    double rho = cblas_ddot(n,p,1,p,1);

    int iter = 0;
    for (; iter < maxit && sqrt(rho) > tol; ++iter)
    {
        /* q = A p  */
        armpl_spmv_exec_d(ARMPL_SPARSE_OPERATION_NOTRANS, 1.0, A, p, 0.0, q);
        double denom = cblas_ddot(m,q,1,q,1);
        if (denom == 0.0) { iter = -3; break; }

        double alpha = rho / denom;
        cblas_daxpy(n,  alpha, p,1, x,1);     /* x += α p */
        cblas_daxpy(m, -alpha, q,1, r,1);     /* r -= α q */

        armpl_spmv_exec_d(ARMPL_SPARSE_OPERATION_TRANS, 1.0, A, r, 0.0, At);
        double rho_new = cblas_ddot(n, At,1, At,1);

        if (sqrt(rho_new) <= tol) { ++iter; break; }

        double beta = rho_new / rho;
        for(int i=0;i<n;++i) p[i] = At[i] + beta * p[i];
        rho = rho_new;
    }

    /* 後片付け */
    free(r); free(p); free(q); free(At);
    armpl_spmat_destroy(A);

    if (iter >= maxit) return -2;    /* 収束せず */
    if (iter < 0)       return -3;   /* API 失敗 */
    return iter;                     /* 収束回数 */
}

