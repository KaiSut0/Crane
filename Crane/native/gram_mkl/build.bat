@echo off
call "C:\Program Files (x86)\Intel\oneAPI\setvars.bat" intel64

cl /O2 /LD /MD /Iinclude src\gram_mkl.cpp ^
   mkl_intel_lp64.lib mkl_tbb_thread.lib mkl_core.lib tbb12.lib ^
   /Fe:gram.dll

cl /O2 /MD /Iinclude test\test_gram.cpp gram_mkl.lib
