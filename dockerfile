# 1. Build Stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy solution and project files
COPY WeddingApp.sln .
COPY Wedding.Model/Wedding.Model.csproj Wedding.Model/
COPY Wedding.Repository/Wedding.Repository.csproj Wedding.Repository/
COPY WeddingApp.UI/WeddingApp.UI.csproj WeddingApp.UI/

# Restore dependencies
RUN dotnet restore

# Copy everything else
COPY . .

# Publish application
RUN dotnet publish WeddingApp.UI/WeddingApp.UI.csproj -c Release -o /app/publish

# 2. Runtime Stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish .
EXPOSE 80
ENTRYPOINT ["dotnet", "WeddingApp.UI.dll"]
