FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

COPY backend/OneClickYatra.MigrationRunner/OneClickYatra.MigrationRunner.csproj backend/OneClickYatra.MigrationRunner/
RUN dotnet restore backend/OneClickYatra.MigrationRunner/OneClickYatra.MigrationRunner.csproj

COPY backend/OneClickYatra.MigrationRunner/ backend/OneClickYatra.MigrationRunner/
COPY database/ database/
RUN dotnet publish backend/OneClickYatra.MigrationRunner/OneClickYatra.MigrationRunner.csproj -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/runtime:9.0
WORKDIR /app
COPY --from=build /app/publish .
COPY database/ /app/database/
ENTRYPOINT ["dotnet", "OneClickYatra.MigrationRunner.dll"]
