#define CGNRMKL_EXPORTS
#include "../include/cgnr_mkl.h"

#include <mkl.h>
#include <cmath>
#include <cstdlib>
#include <cstring>


enum { OK = 0, ERR_MKL = -1, ERR_ALLOC = -2, NO_CONV = 1 };

static double dot(int n, const double* x, const double* y)
{
    return cblas_ddot(n, x, 1, y, 1);
}

/* ---------------------------------------------------------------- */
extern "C" DLL_API int
cgnr_solve_csr_double(
        int   m, int n,
        const int*    Ap,
        const int*    Aj,
        const double* Ax,
        const double* b,
        double*       x,
        int   maxIter,
        double tol)
{
    if(m<=0||n<=0||!Ap||!Aj||!Ax||!b||!x) return ERR_ALLOC;

    /* --- MKL Sparse handle ------------------------------------ */
    sparse_matrix_t A;
    if(mkl_sparse_d_create_csr(&A, SPARSE_INDEX_BASE_ZERO,
            m, n,
            const_cast<int*>(Ap),
            const_cast<int*>(Ap+1),
            const_cast<int*>(Aj),
            const_cast<double*>(Ax)) != SPARSE_STATUS_SUCCESS)
        return ERR_MKL;

    matrix_descr desc; desc.type = SPARSE_MATRIX_TYPE_GENERAL;

    /* --- work vectors ----------------------------------------- */
    double *r  = (double*)mkl_malloc(m*sizeof(double), 64);
    double *z  = (double*)mkl_malloc(n*sizeof(double), 64);
    double *p  = (double*)mkl_malloc(n*sizeof(double), 64);
    double *q  = (double*)mkl_malloc(m*sizeof(double), 64);
    if(!r||!z||!p||!q){ mkl_free(r);mkl_free(z);mkl_free(p);mkl_free(q);
        mkl_sparse_destroy(A); return ERR_ALLOC; }

    std::memset(x, 0, n*sizeof(double));
    std::memcpy(r, b, m*sizeof(double));          /* r = b          */
    /* z = Aᵀ r */
    if(mkl_sparse_d_mv(SPARSE_OPERATION_TRANSPOSE, 1.0, A, desc,
                       r, 0.0, z) != SPARSE_STATUS_SUCCESS)
        { mkl_free(r);mkl_free(z);mkl_free(p);mkl_free(q); mkl_sparse_destroy(A); return ERR_MKL; }
    std::memcpy(p, z, n*sizeof(double));          /* p = z          */

    double bNorm = sqrt(dot(m,b,b));  if(bNorm==0) bNorm=1.0;
    double rho   = dot(n,z,z);

    int k=0;
    for(; k<maxIter; ++k)
    {
        /* q = A p */
        if(mkl_sparse_d_mv(SPARSE_OPERATION_NON_TRANSPOSE, 1.0, A, desc,
                           p, 0.0, q) != SPARSE_STATUS_SUCCESS)
            { k = -ERR_MKL; break; }

        double denom = dot(m,q,q);
        if(denom==0){ k = -ERR_MKL; break; }

        double alpha = rho / denom;

        /* x += alpha p */
        cblas_daxpy(n, alpha, p, 1, x, 1);

        /* r -= alpha q */
        cblas_daxpy(m, -alpha, q, 1, r, 1);

        /* convergence */
        double rNorm = sqrt(dot(m,r,r));
        if(rNorm / bNorm < tol) { k++; break; }

        /* z = Aᵀ r */
        if(mkl_sparse_d_mv(SPARSE_OPERATION_TRANSPOSE, 1.0, A, desc,
                           r, 0.0, z) != SPARSE_STATUS_SUCCESS)
            { k = -ERR_MKL; break; }

        double rho_new = dot(n,z,z);
        double beta = rho_new / rho;
        rho = rho_new;

        /* p = z + beta p */
        cblas_dscal(n, beta, p, 1);
        cblas_daxpy(n, 1.0, z, 1, p, 1);
    }

    /* clean */
    mkl_free(r); mkl_free(z); mkl_free(p); mkl_free(q);
    mkl_sparse_destroy(A);

    return (k<=0 || k>maxIter) ? NO_CONV : OK;
}


/* =============================================================== *
 *  Conjugate Gradient  (SPD n×n, 0-based CSR)                     *
 * =============================================================== */
 extern "C" DLL_API
 int cg_solve_csr_double(
     int n,
     const int* Ap,const int* Aj,const double* Ax,
     const double* b,double* x,
     int maxIter,double tol)
 {
     if(n<=0||!Ap||!Aj||!Ax||!b||!x) return -1;
 
     sparse_matrix_t A;
     if(mkl_sparse_d_create_csr(&A,SPARSE_INDEX_BASE_ZERO,
             n,n,
             const_cast<int*>(Ap),
             const_cast<int*>(Ap+1),
             const_cast<int*>(Aj),
             const_cast<double*>(Ax))!=SPARSE_STATUS_SUCCESS)
         return -2;
     /* SPD なので triangular hintsを付けると少し速くなる */
     matrix_descr desc{SPARSE_MATRIX_TYPE_SYMMETRIC,
                       SPARSE_FILL_MODE_UPPER, /* 上三角 CSR と仮定 */
                       SPARSE_DIAG_NON_UNIT};
 
     double *r=(double*)mkl_malloc(n*sizeof(double),64);
     double *p=(double*)mkl_malloc(n*sizeof(double),64);
     double *Apv=(double*)mkl_malloc(n*sizeof(double),64);
     if(!r||!p||!Apv){ mkl_sparse_destroy(A); return -3;}
 
     std::memset(x,0,n*sizeof(double));
     std::memcpy(r,b,n*sizeof(double));            /* r=b-Ax0 */
 
     std::memcpy(p,r,n*sizeof(double));            /* p=r     */
     double rsold = cblas_ddot(n,r,1,r,1);
     double bnorm = std::sqrt(rsold); if(bnorm==0) bnorm=1;
 
     for(int k=0;k<maxIter;++k)
     {
         /* Ap = A p */
         mkl_sparse_d_mv(SPARSE_OPERATION_NON_TRANSPOSE,
                         1.0,A,desc,p,0.0,Apv);
 
         double alpha = rsold / cblas_ddot(n,p,1,Apv,1);
 
         /* x += α p */
         cblas_daxpy(n, alpha, p,1, x,1);
 
         /* r -= α Ap */
         cblas_daxpy(n,-alpha,Apv,1, r,1);
 
         double rsnew = cblas_ddot(n,r,1,r,1);
         if(std::sqrt(rsnew)/bnorm < tol)
         { mkl_free(r);mkl_free(p);mkl_free(Apv); mkl_sparse_destroy(A); return 0; }
 
         double beta = rsnew / rsold;
         rsold = rsnew;
 
         /* p = r + β p */
         cblas_dscal(n,beta,p,1);
         cblas_daxpy(n,1.0,r,1,p,1);
     }
     mkl_free(r);mkl_free(p);mkl_free(Apv); mkl_sparse_destroy(A);
     return 1;           /* 未収束 */
 }