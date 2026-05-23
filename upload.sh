#!/bin/bash

BUCKET="unity-development.gotchiverse.io"
BUILD_PATH="./Builds/WebGL"

# Sync .wasm files
aws s3 sync "$BUILD_PATH" "s3://$BUCKET" \
  --exclude "*" \
  --include "*.wasm" \
  --content-type "application/wasm" \

# Sync .data files
aws s3 sync "$BUILD_PATH" "s3://$BUCKET" \
  --exclude "*" \
  --include "*.data" \
  --content-type "application/octet-stream" \

# Sync .js files
aws s3 sync "$BUILD_PATH" "s3://$BUCKET" \
  --exclude "*" \
  --include "*.js" \
  --content-type "application/javascript" \

# Sync everything else (HTML, etc)
aws s3 sync "$BUILD_PATH" "s3://$BUCKET" \
  --exclude "*.wasm" \
  --exclude "*.data" \
  --exclude "*.js" \