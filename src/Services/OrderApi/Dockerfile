#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:7.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443
ARG CERTS_PATH=srv/buildcertificate/rootca


FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
WORKDIR /src
COPY ["Services/OrderApi/OrderApi.csproj", "Services/OrderApi/"]
RUN dotnet restore "Services/OrderApi/OrderApi.csproj"
COPY . .
WORKDIR "/src/Services/OrderApi"
RUN dotnet build "OrderApi.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "OrderApi.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .

#RUN mkdir -p /https/ 
#COPY $CERTS_PATH /https
#RUN cp -R /https/* /usr/local/share/ca-certificates
#RUN update-ca-certificates -f

ENTRYPOINT ["dotnet", "OrderApi.dll"]