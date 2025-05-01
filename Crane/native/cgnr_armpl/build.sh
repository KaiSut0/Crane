ARMPL_DIR=/opt/arm/armpl_25.04_flang-new_clang_19

mkdir -p build && cd build
cp ../lib/libarmpl_lp64.dylib ./

clang -std=c11 -O3 -fvisibility=hidden \
      -I../include -I$ARMPL_DIR/include \
      -c ../src/cgnr_solver.c \
      -c ../src/cg_solver.c      

clang -shared -o libcgnr.dylib \
      cgnr_solver.o cg_solver.o \
      -L./ -larmpl_lp64 -lpthread -lm \
      -Wl,-install_name,@rpath/libcgnr.dylib \
      -Wl,-rpath,@loader_path