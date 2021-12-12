#!/bin/bash

dockername="solidtrade-dev-server";
port=8007;

if [ "$1" = "production" ]; then
  dockername="solidtrade-server";
  port=8008;
fi

python3 patch_appsettings.py '~/projects/Rose-Linode/env/SolidTrade/server/appsettings.json'

# Stop current docker image
docker stop $(docker ps | awk '{split($2,image,":"); print $1, image[1]}' | awk -v image=$dockername '$2 == image {print $1}')

# Build & Run Image
docker build -t $dockername . && docker run -it -d -p $port:80 $dockername && docker ps