#include "../include/cgnr_mkl.h"
#include <cstdio>
#include <vector>

int main()
{
    /* A = [[3 1];[0 4];[2 0]] (m=3, n=2) */
    int m=3, n=2;
    int Ap[]={0,2,3,4};
    int Aj[]={0,1,1,0};
    double Ax[]={3,1,4,2};

    double b[]={5,16,6};
    std::vector<double> x(n,0.0);

    int rc = cgnr_solve_csr_double(
        m,n, Ap,Aj,Ax,
        b, x.data(),
        1000, 1e-8);

    if(rc!=0){ std::printf("CGNR failed %d\n",rc); return 1; }
    std::printf("x = [%.6f, %.6f]\n", x[0],x[1]); // ≈ [1,3.75]
}