FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

RUN apt-get update && apt-get install -y python3 && ln -sf /usr/bin/python3 /usr/bin/python

RUN dotnet workload install wasm-tools

COPY Hum.sln .
COPY Server/Hum.Server.csproj Server/
COPY Client/Hum.Client.csproj Client/
COPY Shared/Hum.Shared.csproj Shared/

RUN dotnet restore Server/Hum.Server.csproj

COPY Server/ Server/
COPY Client/ Client/
COPY Shared/ Shared/

RUN dotnet publish Server/Hum.Server.csproj -c Release -o /app/publish --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish .

RUN mkdir -p /data

EXPOSE 8080

ENTRYPOINT ["dotnet", "Hum.Server.dll"]
