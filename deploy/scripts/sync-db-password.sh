#!/usr/bin/env bash
# Aligne le mot de passe de l'utilisateur Postgres "jewel" sur POSTGRES_PASSWORD du .env.
#
# Postgres ne lit POSTGRES_PASSWORD qu'à la création de la base : ensuite, le mot de passe
# est stocké DANS la base. Ce script l'y réécrit à chaque déploiement, pour que le secret
# GitHub reste la source de vérité. Sans effet si la valeur n'a pas changé.
#
# Variable optionnelle : COMPOSE_DIR (défaut : /srv/jewel-app)

set -euo pipefail

COMPOSE_DIR="${COMPOSE_DIR:-/srv/jewel-app}"
cd "$COMPOSE_DIR"

# Charge les variables du .env (set -a : les exporte automatiquement).
set -a; . ./.env; set +a

: "${POSTGRES_PASSWORD:?POSTGRES_PASSWORD absent du .env}"
case "$POSTGRES_PASSWORD" in
  *'$pw$'*) echo "POSTGRES_PASSWORD ne doit pas contenir la séquence \$pw\$" >&2; exit 1 ;;
esac

# $pw$...$pw$ : chaîne "dollar-quotée" de Postgres, le mot de passe peut contenir
# des apostrophes sans casser la requête. La requête passe par l'entrée standard :
# le mot de passe n'apparaît pas dans la liste des processus.
# psql se connecte par le socket local du conteneur, qui n'exige pas de mot de passe :
# ça marche même quand l'ancien mot de passe n'est plus connu.
printf 'ALTER ROLE jewel WITH PASSWORD $pw$%s$pw$;\n' "$POSTGRES_PASSWORD" \
  | docker compose exec -T db psql --quiet -v ON_ERROR_STOP=1 --username=jewel --dbname=jewelryshop

echo "Mot de passe Postgres synchronisé avec le .env"
