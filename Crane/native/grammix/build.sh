#!/usr/bin/env bash
set -e
# ArmPL ルートを環境変数で渡す。未設定なら☞Homebrew 例
ARMPL=${ARMPL_DIR:-/opt/arm/armpl_25.04_flang-new_clang_19}

clang -std=c11 -O3 -fvisibility=hidden \
      -Iinclude -I${ARMPL}/include \
      -c src/gram25.c

# macOS → dylib, Linux → so （名前は自由）
OUT=libgram25.dylib
clang -shared -o $OUT gram25.o \
      -L${ARMPL}/lib -larmpl_lp64 -lpthread -lm \
      -Wl,-rpath,${ARMPL}/lib
echo "built $OUT"