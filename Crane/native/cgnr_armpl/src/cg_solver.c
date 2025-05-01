#include <math.h>
#include <stdlib.h>
#include <string.h>
#include "armpl.h"
#include "../include/cgnr_solver.h"

/* CSR→ArmPL ハンドル生成は再利用 --------------------------- */
extern armpl_spmat_t
    create_csr_d(int m,int n,const int*,const int*,const double*);  /* 宣言だけ */

#define NEWVEC(v,n)  do{ (v)=calloc((n),sizeof(double)); if(!(v)) return -1; }while(0)

int cg_solve_lp64(int n,
                  const int* rowptr, const int* colind, const double* val,
                  const double* b, double* x,
                  double tol, int maxit)
{
    armpl_spmat_t A = create_csr_d(n,n,rowptr,colind,val);
    if(!A) return -1;

    double *r,*p,*Ap;
    NEWVEC(r,n); NEWVEC(p,n); NEWVEC(Ap,n);

    /* r0 = b - A·x0  (x0 = 0 を想定) */
    memcpy(r,b,n*sizeof(double));
    memcpy(p,r,n*sizeof(double));           /* p0 = r0 */
    armpl_spmv_exec_d(ARMPL_SPARSE_OPERATION_NOTRANS, -1.0, A, x, 0.0, r);
    cblas_daxpy(n, 1.0, b, 1, r, 1);

    double rsold = cblas_ddot(n,r,1,r,1);

    int k=0;
    for(; k<maxit && sqrt(rsold) > tol; ++k)
    {
        armpl_spmv_exec_d(ARMPL_SPARSE_OPERATION_NOTRANS,
                          1.0,A,p,0.0,Ap);          /* Ap = A p */
        double alpha = rsold / cblas_ddot(n,p,1,Ap,1);

        /* x  = x + α p */
        cblas_daxpy(n, alpha, p,1, x,1);
        /* r  = r - α Ap */
        cblas_daxpy(n,-alpha,Ap,1, r,1);

        double rsnew = cblas_ddot(n,r,1,r,1);
        if (sqrt(rsnew) <= tol) { ++k; break; }

        double beta = rsnew / rsold;
        for(int i=0;i<n;++i) p[i] = r[i] + beta*p[i];

        rsold = rsnew;
    }

    free(r); free(p); free(Ap);
    armpl_spmat_destroy(A);

    if (k>=maxit) return -2;
    return k;                  /* 収束回数を返す */
}