#!/bin/sh
mkdir ./flutter-sdk && cd ./flutter-sdk

git clone -b flutter-2.8-candidate.20 https://github.com/flutter/flutter.git

export PATH="$PATH:`pwd`/flutter/bin"

cd ../client/

ls

flutter build web --release
