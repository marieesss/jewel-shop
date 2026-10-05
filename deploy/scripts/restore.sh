#!/usr/bin/env bash
# Restauration de la base JewelryShop depuis une sauvegarde de backup.sh.
# REMPLACE le contenu actuel de la base. Une sauvegarde de l'état actuel est faite avant.
#
#   restore.sh /srv/jewel-app/backups/jewelryshop-20261005-033000-daily.dump
#
# Variable optionnelle : COMPOSE_DIR (défaut : /srv/jewel-app)

set -euo pipefail

FILE="${1:?usage : restore.sh <fichier.dump>}"
COMPOSE_DIR="${COMPOSE_DIR:-/srv/jewel-app}"
SCRIPT_DIR="$(cd "$(dirname "$0")" && pwd)"

[ -f "$FILE" ] || { echo "Fichier introuvable : $FILE" >&2; exit 1; }
FILE="$(cd "$(dirname "$FILE")" && pwd)/$(basename "$FILE")"   # chemin absolu
cd "$COMPOSE_DIR"

echo "La base jewelryshop va être REMPLACÉE par : $FILE"
read -rp "Taper RESTAURER pour confirmer : " ANSWER
[ "$ANSWER" = "RESTAURER" ] || { echo "Annulé."; exit 1; }

# Filet de sécurité : on sauvegarde l'état actuel avant de l'écraser.
COMPOSE_DIR="$COMPOSE_DIR" "$SCRIPT_DIR/backup.sh" pre-restore

# L'API est arrêtée pour qu'aucune écriture n'arrive pendant la restauration.
docker compose stop api

# --clean --if-exists : supprime les objets existants avant de les recréer.
# --no-owner          : tout appartient à l'utilisateur qui restaure (jewel).
# --single-transaction : tout ou rien ; en cas d'erreur, la base reste comme avant.
# trap : l'API redémarre même si pg_restore échoue.
trap 'docker compose start api' EXIT
docker compose exec -T db pg_restore --username=jewel --dbname=jewelryshop \
  --clean --if-exists --no-owner --single-transaction < "$FILE"

echo "Restauration terminée. Redémarrage de l'API..."
