./build.sh                         # ライブラリ生成
clang test/test_gram25.c -Iinclude -L. -lgram25 \
      -Wl,-rpath,@loader_path -o run_gram25
./run_gram25