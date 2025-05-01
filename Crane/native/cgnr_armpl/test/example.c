#include <stdio.h>
#include "../include/cgnr_solver.h"

int main(void)
{
    /* 2×2 のテスト行列 [[4,1],[1,3]] */
    int    m=2,n=2,rowptr[3]={0,2,4},col[4]={0,1,0,1};
    double val[4]={4,1,1,3}, b[2]={1,2}, x[2]={0,0};

    int it = cgnr_solve_lp64(m,n,rowptr,col,val,b,x,1e-12,100);
    printf("iter=%d  x=[%g,%g]\n", it, x[0], x[1]);
    return 0;
}

