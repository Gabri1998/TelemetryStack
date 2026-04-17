FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /app

COPY . ./
WORKDIR /app/apps/telemetry-service

RUN dotnet restore
RUN dotnet publish -c Release -o out

FROM mcr.microsoft.com/dotnet/aspnet:10.0
WORKDIR /app

COPY --from=build /app/apps/telemetry-service/out .

EXPOSE 5001

ENTRYPOINT ["dotnet", "telemetry-service.dll"]