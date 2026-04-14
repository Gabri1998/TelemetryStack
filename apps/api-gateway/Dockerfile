FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /app

COPY . ./
WORKDIR /app/apps/api-gateway

RUN dotnet restore
RUN dotnet publish -c Release -o out

FROM mcr.microsoft.com/dotnet/aspnet:10.0
WORKDIR /app

COPY --from=build /app/apps/api-gateway/out .

EXPOSE 5000

ENTRYPOINT ["dotnet", "api-gateway.dll"]