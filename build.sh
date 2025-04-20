#!/bin/sh
set -e

BUILD_CONFIG="${BUILD_CONFIG:=Debug}"

if [ "$1" = "--watch" ]; then
  dothet watch --project src/Host/Host.csproj
elif [ "$1" = "--publish" ]; then
  rm -rf ./dist
  dotnet restore ModularMonolith.sln
  dotnet build --configuration $BUILD_CONFIG --no-restore
  dotnet publish src/Host/Host.csproj --configuration $BUILD_CONFIG --no-build --no-restore --output ./dist
else
  dotnet build --configuration $BUILD_CONFIG
fi
