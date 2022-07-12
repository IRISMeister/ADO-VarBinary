#!/bin/bash

cp -fR /source/ADO-Writer .
cp -fR lib ADO-Writer/
dotnet restore ADO-Writer/ADO.csproj
dotnet publish ADO-Writer/ADO.csproj -c debug -o /app-ADO-Writer

cp -fR /source/ADO-Reader .
cp -fR lib ADO-Reader/
dotnet restore ADO-Reader/ADO.csproj
dotnet publish ADO-Reader/ADO.csproj -c debug -o /app-ADO-Reader

echo "docker-compose exec dotnet dotnet /app-ADO-Writer/ADO.dll to run"
echo "docker-compose exec dotnet dotnet /app-ADO-Reader/ADO.dll to run"
echo
