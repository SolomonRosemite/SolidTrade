#!/bin/sh
mkdir ./flutter-sdk && cd ./flutter-sdk

# Add Flutter
git clone -b 3.7.3 https://github.com/flutter/flutter/

# Add flutter to path
export PATH="$PATH:`pwd`/flutter/bin"

flutter doctor

cd ../client/

# Create directory if it doesn't exist
mkdir -p ./assets/config

# Write out the environment variable configuration as a json file
echo $App_Config | base64 -di > ./assets/config/app_config.json

# Write out firebase credentials as js file
echo $Firebase_Credentials | base64 -di > ./web/credentials.js

# Write out firebase messaging credentials as js file
echo $Firebase_Messaging_Credentials | base64 -di > ./web/firebase-messaging-sw.js

# Install dependencies
flutter pub get

# Generate mappings
flutter pub run build_runner build

# Build web app
if [ "$Deployment" = "Production" ];
then
  flutter build web --release
fi
