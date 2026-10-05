# syntax=docker/dockerfile:1
# Image de l'API JewelryShop. Contexte de build : le dossier jewel-api/.

# ── Étape 1 : build ───────────────────────────────────────────────────────────
# Le SDK (~800 Mo) sert uniquement à compiler ; il ne part pas en production.
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Restauration NuGet seule d'abord : cette couche reste en cache tant que
# le .csproj ne change pas, même si le code change.
COPY src/JewelryShop.API/JewelryShop.API.csproj src/JewelryShop.API/
RUN dotnet restore src/JewelryShop.API/JewelryShop.API.csproj

COPY src/ src/
COPY db/ db/
RUN dotnet publish src/JewelryShop.API/JewelryShop.API.csproj \
      --configuration Release \
      --no-restore \
      --output /app/publish \
      /p:UseAppHost=false

# ── Étape 2 : runtime ─────────────────────────────────────────────────────────
# Le runtime ASP.NET seul (~220 Mo) : pas de compilateur, pas de code source.
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime

# curl uniquement pour le HEALTHCHECK (l'image de base n'a ni curl ni wget).
RUN apt-get update \
 && apt-get install -y --no-install-recommends curl \
 && rm -rf /var/lib/apt/lists/*

WORKDIR /app
COPY --from=build /app/publish .

# Les images .NET 8 écoutent sur 8080 et fournissent un utilisateur non-root "app".
ENV ASPNETCORE_ENVIRONMENT=Production
EXPOSE 8080
USER $APP_UID

HEALTHCHECK --interval=30s --timeout=5s --start-period=30s --retries=3 \
  CMD curl --fail --silent http://127.0.0.1:8080/health/ready || exit 1

ENTRYPOINT ["dotnet", "JewelryShop.API.dll"]
