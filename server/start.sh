#!/bin/bash

dockername="solidtrade-dev-server";
port=8007;

if [ "$1" = "production" ]; then
  dockername="solidtrade-server";
  port=8008;
fi

# Stop current docker image
docker stop $(docker ps | awk '{split($2,image,":"); print $1, image[1]}' | awk -v image=$dockername '$2 == image {print $1}')

# Build & Start container
if [ "$1" = "production" ]; 
then
    cp '/root/projects/Rose-Linode/env/SolidTrade/server/appsettings.credentials.json' .
    cp '/root/projects/Rose-Linode/env/SolidTrade/server/solid-trade-firebase-credentials.json' .
    docker build --build-arg ENVIRONMENT=Production -t $dockername . && docker run -it -d -p $port:80 $dockername && docker ps
else
  cp '/root/projects/Rose-Linode/env/SolidTrade-Dev/server/appsettings.credentials.json' .
  cp '/root/projects/Rose-Linode/env/SolidTrade-Dev/server/solid-trade-firebase-credentials.json' .
  docker build --build-arg ENVIRONMENT=Staging -t $dockername . && docker run -it -d -p $port:80 $dockername && docker ps
fi