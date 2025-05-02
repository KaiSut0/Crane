#include <stdio.h>
#include <stdlib.h>
#include "../include/gram25.h"

static void print_csr(int n,
                      const int *ptr,const int *col,const double *val)
{
    for(int i=0;i<n;++i){
        printf("row%d:",i);
        for(int k=ptr[i];k<ptr[i+1];++k)
            printf(" (%d,%.4g)",col[k],val[k]);
        puts("");
    }
}

int main(void)
{
    /* A = [[1 2],[0 3]] */
    int Ap[]={0,2,3},  Ac[]={0,1,1}; double Av[]={1,2,3};
    /* B = [[4 0],[0 5]] */
    int Bp[]={0,1,2},  Bc[]={0,1};   double Bv[]={4,5};

    int *Cp,*Cc; double *Cv;
    int rc = gram25_build_lp64(
            2,2, Ap,Ac,Av,
            2,   Bp,Bc,Bv,
            3.0,
            &Cp,&Cc,&Cv);
    if(rc){ fprintf(stderr,"gram25 error %d\n",rc); return 1; }

    print_csr(2,Cp,Cc,Cv);   /* 期待値 row0:(0,11)(1,0.667) row1:(0,6)(1,20.333) */

    free(Cp); free(Cc); free(Cv);
    return 0;
}
