@echo off
:: oneAPI 2025 環境変数
call "C:\Program Files (x86)\Intel\oneAPI\setvars.bat" intel64

:: ---------- DLL (LP64+TBB) -------------
cl /O2 /LD /MD /EHsc /Iinclude src\cgnr_mkl.cpp ^
   mkl_intel_lp64.lib mkl_tbb_thread.lib mkl_core.lib ^
   tbb12.lib ^
   /Fe:cgnr_mkl.dll

:: ---------- TEST -----------------------
cl /O2 /MD /EHsc /Iinclude test\test_cgnr.cpp cgnr_mkl.lib