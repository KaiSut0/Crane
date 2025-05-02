#include <cstdlib>
#include <cstdio>
#include "../include/gram_mkl.h"

int main(){
    int Ap[]={0,2,3},Aj[]={0,1,1}; double Ax[]={1,2,3};
    int Bp[]={0,1,2},Bj[]={0,1};   double Bx[]={4,5};

    int *Cp,*Cj; double *Cx;
    int rc=gram_mkl_build_lp64(2,2,Ap,Aj,Ax, 2,Bp,Bj,Bx, 3.0,
                               &Cp,&Cj,&Cx);
    if(rc){ printf("err %d\n",rc); return 1; }

    printf("row0: (%d,%g) (%d,%g)\n",Cj[0],Cx[0],Cj[1],Cx[1]);
    printf("row1: (%d,%g) (%d,%g)\n",Cj[2],Cx[2],Cj[3],Cx[3]);

    std::free(Cp); std::free(Cj); std::free(Cx);
    return 0;
}
