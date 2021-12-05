#!/bin/bash

# path = "SolidTrade-Dev";
dockername="solidtrade-dev-server";
port=8007;

if [ "$1" = "production" ]; then
#   path = "SolidTrade";
  dockername="solidtrade-server";
  port=8008;
fi

# cp /root/projects/Rose-Linode/env/$path/server/.env .

# Stop current docker image
docker rm $(docker stop $(docker ps -a -q --filter ancestor=$dockername --format="{{.ID}}"))

# Build & Run Image
docker build -t $dockername . && docker run -it -d -p $port:80 $dockername && docker ps