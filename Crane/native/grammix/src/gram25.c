/********************************************************************
*  gram25.c  (Arm Performance Libraries 25.04 / lp64 / double)     *
********************************************************************/
#include <stdlib.h>
#include <string.h>
#include "armpl.h"
#include "../include/gram25.h"

/* エラーコード */
enum { OK=0, E_ALLOC=-1, E_ARMPL=-2, E_ARG=-3 };

/* ---- helper: CSR → ArmPL handle -------------------------------- */
static armpl_spmat_t to_handle(
        int m,int n,
        const int* rp,const int* ci,const double* va)
{
    armpl_spmat_t h;
    if(armpl_spmat_create_csr_d(&h,
            (armpl_int_t)m,(armpl_int_t)n,
            (const armpl_int_t*)rp,
            (const armpl_int_t*)ci,
            va, 0))
        return NULL;
    return h;
}

/* ---- helper: ArmPL CSR → malloc コピー ------------------------- */
static int copy_csr(armpl_spmat_t M,
        int** rp,int** ci,double** va)
{
    armpl_int_t m,n,*row,*col; double* val;
    if(armpl_spmat_export_csr_d(M,0,&m,&n,&row,&col,&val))
        return E_ARMPL;
    armpl_int_t nnz=row[m];

    size_t szR=(size_t)(m+1)*sizeof(int);
    size_t szI=(size_t)nnz*sizeof(int);
    size_t szV=(size_t)nnz*sizeof(double);

    int* R=(int*)malloc(szR);
    int* C=(int*)malloc(szI);
    double* V=(double*)malloc(szV);
    if(!R||!C||!V){ free(R);free(C);free(V); return E_ALLOC; }

    memcpy(R,row,szR); memcpy(C,col,szI); memcpy(V,val,szV);
    *rp=R; *ci=C; *va=V; return OK;
}

/* ---- public API ----------------------------------------------- */
int gram25_build_lp64(
        int mA,int n,const int*Ap,const int*Ac,const double*Av,
        int mB,const int*Bp,const int*Bc,const double*Bv,
        double w,
        int** Cp,int** Cc,double** Cv)
{
    if(w<=0.0 || !Ap || !Bp || n<=0) return E_ARG;

    int rc = OK;
    armpl_spmat_t A=NULL,B=NULL,C=NULL;
    int *row0=NULL;

    /* 1. A,B ハンドル作成 */
    A = to_handle(mA,n,Ap,Ac,Av);
    B = to_handle(mB,n,Bp,Bc,Bv);
    if(!A||!B){ rc=E_ARMPL; goto done; }

    /* 2. 空 C (growable) – row_ptr だけゼロ初期化必須 */
    row0 = (int*)calloc((size_t)n + 1, sizeof(int));
    if(!row0){ rc=E_ALLOC; goto done; }

    armpl_spmat_create_csr_d(&C,
        (armpl_int_t)n,(armpl_int_t)n,
        (const armpl_int_t*)row0,   /* row_ptr 有効 */
        NULL,NULL, 0);

    /* 3. (1/w)·AᵀA */
    armpl_spmm_optimize(
        ARMPL_SPARSE_OPERATION_TRANS,
        ARMPL_SPARSE_OPERATION_NOTRANS,
        ARMPL_SPARSE_SCALAR_ONE,
        A, A, 
        ARMPL_SPARSE_SCALAR_ONE, C);

    armpl_spmm_exec_d(
        ARMPL_SPARSE_OPERATION_TRANS,
        ARMPL_SPARSE_OPERATION_NOTRANS,
        1.0/w, A, A,
        0.0,   C);


    /* 4. ((w-1)/w)·BᵀB → 累積 */
    armpl_spmm_optimize(
        ARMPL_SPARSE_OPERATION_TRANS,
        ARMPL_SPARSE_OPERATION_NOTRANS,
        ARMPL_SPARSE_SCALAR_ONE, 
        B, B, 
        ARMPL_SPARSE_SCALAR_ONE, C);

    armpl_spmm_exec_d(
        ARMPL_SPARSE_OPERATION_TRANS,
        ARMPL_SPARSE_OPERATION_NOTRANS,
        (w-1.0)/w, B, B,
        1.0,           C);

    /* 5. コピーアウト */
    rc = copy_csr(C, Cp, Cc, Cv);

done:
    armpl_spmat_destroy(A);
    armpl_spmat_destroy(B);
    armpl_spmat_destroy(C);
    free(row0);
    if(rc){ *Cp=*Cc=NULL; *Cv=NULL; }
    return rc;
}
