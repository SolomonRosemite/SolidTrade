#!/usr/bin/env bash

branch_name=$1
environment_name=solidtrade-$branch_name
credentials_folder=$environment_name

# Select general development folder for feature branches
if [[ "$branch_name" != "stable" && "$branch_name" != "staging" ]]; then
  credentials_folder="solidtrade-development"
fi

# Copy configuration and credentials 
cp ~/projects/Rose-Linode/env/${credentials_folder}/.env '.'
cp ~/projects/Rose-Linode/env/${credentials_folder}/server/appsettings.credentials.json './server/Configuration'
cp ~/projects/Rose-Linode/env/${credentials_folder}/server/solid-trade-firebase-credentials.json './server/Configuration'

# We do not require ngrok for production. This is why we scale down to 0.
docker compose --project-name $environment_name up -d --force-recreate --scale ngrok=0