#define GRAMMKL_EXPORTS
#include "../include/gram_mkl.h"

#include <mkl.h>
#include <cstdlib>
#include <cstring>

enum { OK=0, ERR_MKL=-1, ERR_ALLOC=-2, ERR_ARG=-3 };

/* CSR -> MKL handle ------------------------------------------------*/
static sparse_matrix_t csr2handle(
        MKL_INT m,MKL_INT n,
        const int* rp,const int* ci,const double* va)
{
    sparse_matrix_t h;
    return (mkl_sparse_d_create_csr(&h,SPARSE_INDEX_BASE_ZERO,
            m,n,
            const_cast<int*>(rp),
            const_cast<int*>(rp+1),
            const_cast<int*>(ci),
            const_cast<double*>(va))==SPARSE_STATUS_SUCCESS)?h:nullptr;
}

/* 深いコピー + 値を factor 倍 -------------------------------------*/
static sparse_matrix_t copy_and_scale(sparse_matrix_t src,double factor)
{
    /* export */
    sparse_index_base_t base;
    MKL_INT rows,cols,*iaStart,*iaEnd,*ja;
    double *val;
    if(mkl_sparse_d_export_csr(src,&base,&rows,&cols,
            &iaStart,&iaEnd,&ja,&val)!=SPARSE_STATUS_SUCCESS)
        return nullptr;

    MKL_INT nnz = iaEnd[rows-1];

    /* malloc copy */
    int* rp=(int*)malloc((rows+1)*sizeof(int));
    int* cj=(int*)malloc(nnz*sizeof(int));
    double* vx=(double*)malloc(nnz*sizeof(double));
    if(!rp||!cj||!vx){ free(rp);free(cj);free(vx); return nullptr; }

    std::memcpy(rp , iaStart, rows*sizeof(int));
    rp[rows]=nnz;
    std::memcpy(cj , ja , nnz*sizeof(int));
    for(MKL_INT i=0;i<nnz;++i) vx[i]=val[i]*factor;

    sparse_matrix_t dst;
    if(mkl_sparse_d_create_csr(&dst,SPARSE_INDEX_BASE_ZERO,
            rows,cols,
            rp,rp+1,cj,vx)!=SPARSE_STATUS_SUCCESS)
    { free(rp);free(cj);free(vx); return nullptr; }

    return dst;   /* MKL が rp/cj/vx を所有。解放は destroy に任せる */
}

/* MKL handle -> malloc CSR (row[n]=nnz) ----------------------------*/
static int export_csr(sparse_matrix_t H,
        int** rp,int** cj,double** vx)
{
    sparse_index_base_t base;
    MKL_INT rows,cols,*iaStart,*iaEnd,*ja; double* val;
    if(mkl_sparse_d_export_csr(H,&base,&rows,&cols,
            &iaStart,&iaEnd,&ja,&val)!=SPARSE_STATUS_SUCCESS)
        return ERR_MKL;

    MKL_INT nnz = iaEnd[rows-1];
    *rp=(int*)malloc((rows+1)*sizeof(int));
    *cj=(int*)malloc(nnz*sizeof(int));
    *vx=(double*)malloc(nnz*sizeof(double));
    if(!*rp||!*cj||!*vx) return ERR_ALLOC;

    std::memcpy(*rp, iaStart, rows*sizeof(int));
    (*rp)[rows]=nnz;
    std::memcpy(*cj, ja , nnz*sizeof(int));
    std::memcpy(*vx, val, nnz*sizeof(double));
    return OK;
}

/*==================================================================*/
extern "C"
DLL_API int gram_mkl_build_lp64(
    int mA,int n,const int*Ap,const int*Aj,const double*Ax,
    int mB,const int*Bp,const int*Bj,const double*Bx,
    double w,
    int** Cp,int** Cj,double** Cv)
{
    if(w<=0.0||!Ap||!Bp||n<=0) return ERR_ARG;

    sparse_matrix_t A=nullptr,B=nullptr,AtA=nullptr,BtB=nullptr;
    sparse_matrix_t C1=nullptr,C2=nullptr,C=nullptr;
    matrix_descr desc{SPARSE_MATRIX_TYPE_GENERAL};
    int rc=OK;

    /* handles */
    if(!(A=csr2handle(mA,n,Ap,Aj,Ax))) rc=ERR_MKL;
    if(!(B=csr2handle(mB,n,Bp,Bj,Bx))) rc=ERR_MKL;
    if(rc) goto done;

    /* AtA */
    if(mkl_sparse_sp2m(SPARSE_OPERATION_TRANSPOSE,desc,A,
                       SPARSE_OPERATION_NON_TRANSPOSE,desc,A,
                       SPARSE_STAGE_FULL_MULT,&AtA))
        { rc=ERR_MKL; goto done; }

    /* BtB */
    if(mkl_sparse_sp2m(SPARSE_OPERATION_TRANSPOSE,desc,B,
                       SPARSE_OPERATION_NON_TRANSPOSE,desc,B,
                       SPARSE_STAGE_FULL_MULT,&BtB))
        { rc=ERR_MKL; goto done; }

    /* スケール付きコピー */
    C1 = copy_and_scale(AtA, 1.0/w);
    C2 = copy_and_scale(BtB, (w-1.0)/w);
    if(!C1||!C2){ rc=ERR_MKL; goto done; }

    /* C = C1 + C2  (mkl_sparse_d_add は 5 引数) */
    if(mkl_sparse_d_add(SPARSE_OPERATION_NON_TRANSPOSE,
                        C1, 1.0, C2, &C))
        { rc=ERR_MKL; goto done; }

    /* export */
    rc = export_csr(C,Cp,Cj,Cv);

done:
    mkl_sparse_destroy(A);  mkl_sparse_destroy(B);
    mkl_sparse_destroy(AtA);mkl_sparse_destroy(BtB);
    mkl_sparse_destroy(C1); mkl_sparse_destroy(C2);
    mkl_sparse_destroy(C);
    if(rc){ *Cp=*Cj=nullptr; *Cv=nullptr; }
    return rc;
}
