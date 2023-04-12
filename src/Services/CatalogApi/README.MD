0) Add to host file for postgres connection from dev
YourLocalIP localhost
Example:
192.168.0.15 localhost

1) Added https://learn.microsoft.com/en-us/aspnet/core/data/ef-mvc/intro?view=aspnetcore-7.0
Install-Package Microsoft.EntityFrameworkCore.Tools
https://www.learnentityframeworkcore.com/configuration/data-annotation-attributes
Install-Package Microsoft.VisualStudio.Web.CodeGeneration.Design


2) add-migration InitialCreate -StartupProject "Services\CatalogApi" -Project "Services\CatalogApi" -OutputDir "Data/Migrations"
-Context"ProductApiContext"

-Context "ApplicationDbContext"
-StartupProject "EntryProject.Name" 
-Project "MigrationProject.Name"

Update-Database
3) https://dev.to/timothymcgrath/logging-scaffold-for-net-core-serilog-3o91
 dotnet add package Serilog.Sinks.Console
 dotnet add package Serilog.Sinks.File
4) https://github.com/serilog/serilog/wiki/Configuration-Basics

5) https://nblumhardt.com/2021/06/customize-serilog-text-output/
          "outputTemplate": "{Timestamp:HH:mm:ss.fff} [{Level:u3}] [{SourceContext}] {Message}{NewLine}{Exception}",
https://stackoverflow.com/questions/66550450/how-to-use-expression-template-with-appsettings-in-serilog

6) dotnet add package Serilog.Sinks.Elasticsearch --version 8.4.1
Install-Package serilog.sinks.elasticsearch
https://github.com/serilog-contrib/serilog-sinks-elasticsearch

"formatter": "Serilog.Formatting.Json.JsonFormatter, Serilog"
docker logs --tail 50 --follow --timestamps 51479fcb93a7

logstash post request
https://subscription.packtpub.com/book/data/9781787281868/10/ch10lvl1sec62/sending-data-to-logstash-over-http

$ curl -sSL https://raw.githubusercontent.com/bitnami/containers/main/bitnami/APP/docker-compose.yml > docker-compose.logstash.yml
$ docker-compose up -d
curl -XPOST -u sensor_data:sensor_data --header "Content-Type: application/json" "http://localhost:31000/" -d '{"sensor_id":1,"time":1512102540000,"reading":16.24}'

https://devconnected.com/how-to-install-logstash-on-ubuntu-18-04-and-debian-9/

https://stackoverflow.com/questions/19086404/how-to-debug-the-logstash-file-plugin

appsettings.json

Migration
Npgsql.EntityFrameworkCore.PostgreSQL
Add-Migration InitialCreate folder

Npgsql.EntityFrameworkCore.PostgreSQL
Add-Migration InitialCreate -OutputDir "Data/Migrations"
Add-Migration название_миграции
Update-Database

https://khalidabuhakmeh.com/how-to-add-a-view-to-an-entity-framework-core-dbcontext

--msbuildprojectextensionspath