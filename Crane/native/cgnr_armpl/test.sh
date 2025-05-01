clang test/example.c -Ibuild/include \
      -Lbuild -lcgnr \
      -Wl,-rpath,./build/ \
      -o run && ./run
#=> iter=6  x=[0.0909091,0.636364]