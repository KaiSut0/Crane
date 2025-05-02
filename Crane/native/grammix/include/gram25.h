#ifndef GRAM25_H_
#define GRAM25_H_

#ifdef __cplusplus
extern "C" { 
#endif

/* 戻り値: 0 成功, 負値 = エラー */
__attribute__((visibility("default")))
int gram25_build_lp64(
    /* A (mA×n) --------------------------------------------------- */
    int mA, int n,
    const int* Ap, const int* Ac, const double* Av,
    /* B (mB×n) --------------------------------------------------- */
    int mB,
    const int* Bp, const int* Bc, const double* Bv,
    /* 重み w > 0 ------------------------------------------------- */
    double w,
    /* 出力 C (n×n, CSR) – malloc されたポインタを返す ---------- */
    int** Cp, int** Cc, double** Cv);

#ifdef __cplusplus
}
#endif
#endif
