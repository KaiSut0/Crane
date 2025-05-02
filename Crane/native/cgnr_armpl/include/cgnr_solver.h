#ifndef CGNR_SOLVER_H_
#define CGNR_SOLVER_H_

#include <stddef.h>     /* size_t */

/* 戻り値 :  >=0 収束した反復回数
 *          -1  行列生成エラー
 *          -2  最大反復で収束せず
 *          -3  ArmPL API 失敗
 */
#ifdef __cplusplus
extern "C" {
#endif

__attribute__((visibility("default")))
int cgnr_solve_lp64(
    /* 行列 A : CSR 形式 ------------------------------------ */
    int          m,           /* 行数 */
    int          n,           /* 列数 */
    const int*   rowptr,      /* サイズ m+1 */
    const int*   colind,      /* サイズ rowptr[m] (=nnz) */
    const double*values,      /* 同上                        */

    /* ベクトル -------------------------------------------- */
    const double* b,          /* サイズ m */
    double*       x,          /* in/out : 初期値→解 (サイズ n) */

    /* パラメータ ------------------------------------------ */
    double        tol,        /* 許容誤差 (例 1e-10) */
    int           maxit       /* 反復上限 (例 1000) */
);


/* ───────────────────────────────────────────────────────── */
__attribute__((visibility("default")))
int cg_solve_lp64(
    int           n,               /* 行数＝列数                */
    const int*    rowptr,          /* CSR ptr (n+1)            */
    const int*    colind,          /* CSR col (nnz)            */
    const double* vals,            /* CSR val (nnz)            */
    const double* b,               /* 右辺 (n)                 */
    double*       x,               /* in/out: 初期値→解 (n)    */
    double        tol,             /* 収束判定 ‖r‖ <= tol      */
    int           maxit            /* 最大反復                 */
);
/* 戻り値 :  >=0  収束した反復回数
 *           -1   行列生成エラー
 *           -2   maxit で収束せず
 *           -3   ArmPL API 失敗
 */


#ifdef __cplusplus
}
#endif
#endif /* CGNR_SOLVER_H_ */

