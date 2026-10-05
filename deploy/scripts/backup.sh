#!/usr/bin/env bash
# Sauvegarde de la base JewelryShop avec pg_dump.
#
#   backup.sh [étiquette]        ex. : backup.sh daily   |   backup.sh pre-deploy-abc1234
#
# Variables optionnelles :
#   COMPOSE_DIR     dossier du compose.yaml   (défaut : /srv/jewel-app)
#   BACKUP_DIR      dossier des sauvegardes   (défaut : $COMPOSE_DIR/backups)
#   RETENTION_DAYS  durée de conservation     (défaut : 14 jours)

set -euo pipefail

LABEL="${1:-manual}"
COMPOSE_DIR="${COMPOSE_DIR:-/srv/jewel-app}"
BACKUP_DIR="${BACKUP_DIR:-$COMPOSE_DIR/backups}"
RETENTION_DAYS="${RETENTION_DAYS:-14}"

umask 077   # sauvegardes lisibles par leur propriétaire uniquement
mkdir -p "$BACKUP_DIR"
cd "$COMPOSE_DIR"

FILE="$BACKUP_DIR/jewelryshop-$(date -u +%Y%m%d-%H%M%S)-$LABEL.dump"

# pg_dump tourne DANS le conteneur db (pas besoin de Postgres sur l'hôte) et écrit sur
# sa sortie standard, redirigée vers un fichier de l'hôte.
#   --format=custom : compressé, et pg_restore peut restaurer tout ou partie.
# Écriture dans un .tmp puis renommage : un fichier .dump est toujours complet.
docker compose exec -T db pg_dump --username=jewel --dbname=jewelryshop --format=custom > "$FILE.tmp"
mv "$FILE.tmp" "$FILE"

# Rotation : supprime les sauvegardes plus vieilles que RETENTION_DAYS jours.
find "$BACKUP_DIR" -name 'jewelryshop-*.dump' -mtime +"$RETENTION_DAYS" -delete

echo "$(date -u +%FT%TZ) sauvegarde OK : $FILE ($(du -h "$FILE" | cut -f1))"
