# 1. Base image (runtime)
FROM mcr.microsoft.com/dotnet/aspnet:7.0 AS base
WORKDIR /app
EXPOSE 80

# 2. Build image (SDK)
FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
WORKDIR /src

# 3. Copy solution & restore
COPY WeddingApp.sln .
COPY Wedding.Model/Wedding.Model.csproj Wedding.Model/
COPY Wedding.Repository/Wedding.Repository.csproj Wedding.Repository/
COPY WeddingApp.UI/WeddingApp.UI.csproj WeddingApp.UI/

RUN dotnet restore

# 4. Copy everything else & publish
COPY . .
RUN dotnet publish WeddingApp.UI/WeddingApp.UI.csproj -c Release -o /app/publish

# 5. Final image
FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "WeddingApp.UI.dll"]
