# syntax=docker/dockerfile:1
# Image du front JewelryShop. Contexte de build : le dossier jewel-front/.

# ── Étape 1 : build ───────────────────────────────────────────────────────────
FROM node:20-alpine AS build
WORKDIR /app

# Dépendances d'abord (cache tant que package*.json ne change pas).
# npm ci installe exactement les versions du package-lock.json.
COPY package.json package-lock.json ./
RUN npm ci

COPY . .

# Vite remplace import.meta.env.VITE_* AU BUILD : la valeur est figée dans le JS.
# Vide = URLs relatives (/api/...) : le front et l'API partagent le même domaine.
ARG VITE_API_URL=""
ENV VITE_API_URL=$VITE_API_URL
RUN npm run build

# ── Étape 2 : serveur statique ────────────────────────────────────────────────
# Variante "unprivileged" : Nginx tourne sans root et écoute sur 8080.
FROM nginxinc/nginx-unprivileged:1.27-alpine AS runtime

COPY nginx.conf /etc/nginx/conf.d/default.conf
COPY --from=build /app/dist /usr/share/nginx/html

EXPOSE 8080

# wget est fourni par busybox dans les images Alpine.
# 127.0.0.1 et non localhost : sous Alpine, localhost se résout d'abord en IPv6 (::1),
# alors que Nginx n'écoute qu'en IPv4 ; busybox wget ne retente pas en IPv4.
HEALTHCHECK --interval=30s --timeout=5s --retries=3 \
  CMD wget --quiet --spider http://127.0.0.1:8080/ || exit 1
