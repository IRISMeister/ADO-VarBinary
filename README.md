# Write/Read binary via ADO

```
$ docker-compose up -d
$ docker-compose exec dotnet /source/dotnet50/build.sh
$ docker-compose exec dotnet dotnet /app-ADO-Writer/ADO.dll
$ docker-compose exec dotnet dotnet /app-ADO-Reader/ADO.dll

$ docker-compose exec dotnet dotnet /app-ADO-Writer/ADO.dll 1024  (1st arg is a size of byte arrays to be written)

```
