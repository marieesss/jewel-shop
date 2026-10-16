#!/usr/bin/env bash
# Crée ou met à jour les Secrets Kubernetes d'un environnement à partir de variables
# d'environnement — dans le pipeline, ce sont les secrets GitHub de l'environnement.
#
#   apply-secrets.sh staging|production
#
# Utilise le kubeconfig courant (celui du ServiceAccount github-deployer dans le pipeline).
# Écrit les empreintes (checksums) des secrets dans $GITHUB_ENV, ou les affiche.

set -euo pipefail

ENVIRONMENT="${1:?usage : apply-secrets.sh staging|production}"
case "$ENVIRONMENT" in staging|production) ;; *) echo "environnement inconnu : $ENVIRONMENT" >&2; exit 1 ;; esac

require() {
  local missing=0 name
  for name in "$@"; do
    if [ -z "${!name:-}" ]; then echo "::error::secret GitHub manquant : $name ($ENVIRONMENT)"; missing=1; fi
  done
  [ "$missing" -eq 0 ] || exit 1
}

# "create --dry-run=client -o yaml | apply" : crée le Secret ou le met à jour.
apply_secret() {
  kubectl create secret "$@" --dry-run=client -o yaml | kubectl apply -f - >/dev/null
  echo "secret/$2 à jour"
}

# Empreinte courte d'un ensemble de valeurs (jamais affichées).
checksum() { printf '%s\n' "$@" | sha256sum | cut -c1-16; }

# ── Commun aux deux environnements ────────────────────────────────────────────
require POSTGRES_PASSWORD JWT_SECRET CLOUDINARY_CLOUD_NAME CLOUDINARY_API_KEY CLOUDINARY_API_SECRET \
        GHCR_PULL_USER GHCR_PULL_TOKEN

apply_secret generic postgres-credentials \
  --from-literal=POSTGRES_PASSWORD="$POSTGRES_PASSWORD"

# Identifiants SMTP (Brevo) facultatifs, hors "require" : sans eux, l'envoi est désactivé et
# les mails restent en attente dans l'outbox, au lieu d'être tentés sans authentification
# puis abandonnés. Email__Enabled est dans ce Secret et non dans le ConfigMap api-config,
# car sa valeur dépend de la présence des identifiants.
EMAIL_SMTP_USERNAME="${EMAIL_SMTP_USERNAME:-}"
EMAIL_SMTP_PASSWORD="${EMAIL_SMTP_PASSWORD:-}"
if [ -n "$EMAIL_SMTP_USERNAME" ] && [ -n "$EMAIL_SMTP_PASSWORD" ]; then
  EMAIL_ENABLED=true
else
  EMAIL_ENABLED=false
  echo "::warning::secrets EMAIL_SMTP_USERNAME / EMAIL_SMTP_PASSWORD absents ou incomplets ($ENVIRONMENT) : envoi de mails désactivé"
fi

CONNECTION_STRING="Host=postgres;Port=5432;Database=jewelryshop;Username=jewel;Password=$POSTGRES_PASSWORD"
apply_secret generic api-secrets \
  --from-literal=ConnectionStrings__DefaultConnection="$CONNECTION_STRING" \
  --from-literal=Jwt__Secret="$JWT_SECRET" \
  --from-literal=Cloudinary__CloudName="$CLOUDINARY_CLOUD_NAME" \
  --from-literal=Cloudinary__ApiKey="$CLOUDINARY_API_KEY" \
  --from-literal=Cloudinary__ApiSecret="$CLOUDINARY_API_SECRET" \
  --from-literal=Email__Enabled="$EMAIL_ENABLED" \
  --from-literal=Email__Smtp__Username="$EMAIL_SMTP_USERNAME" \
  --from-literal=Email__Smtp__Password="$EMAIL_SMTP_PASSWORD"

apply_secret docker-registry ghcr-pull \
  --docker-server=ghcr.io \
  --docker-username="$GHCR_PULL_USER" \
  --docker-password="$GHCR_PULL_TOKEN"

SUM_DB=$(checksum "$POSTGRES_PASSWORD")
SUM_API=$(checksum "$CONNECTION_STRING" "$JWT_SECRET" "$CLOUDINARY_CLOUD_NAME" "$CLOUDINARY_API_KEY" "$CLOUDINARY_API_SECRET" \
                   "$EMAIL_ENABLED" "$EMAIL_SMTP_USERNAME" "$EMAIL_SMTP_PASSWORD")
SUM_PGADMIN=none

# ── Propre à chaque environnement ─────────────────────────────────────────────
# Le hash htpasswd change à chaque calcul (sel aléatoire) : il n'entre pas dans les
# empreintes, et Traefik relit ce Secret de lui-même, sans redémarrage.
if [ "$ENVIRONMENT" = production ]; then
  require PGADMIN_EMAIL PGADMIN_PASSWORD PGADMIN_BASIC_AUTH_USER PGADMIN_BASIC_AUTH_PASSWORD

  apply_secret generic pgadmin-credentials \
    --from-literal=PGADMIN_DEFAULT_EMAIL="$PGADMIN_EMAIL" \
    --from-literal=PGADMIN_DEFAULT_PASSWORD="$PGADMIN_PASSWORD"
  apply_secret generic pgadmin-basic-auth \
    --from-literal=users="$PGADMIN_BASIC_AUTH_USER:$(openssl passwd -apr1 "$PGADMIN_BASIC_AUTH_PASSWORD")"

  # Facultatif : surveillance des sauvegardes (healthchecks.io).
  if [ -n "${BACKUP_HEALTHCHECK_URL:-}" ]; then
    apply_secret generic backup-healthcheck --from-literal=URL="$BACKUP_HEALTHCHECK_URL"
  fi

  SUM_PGADMIN=$(checksum "$PGADMIN_EMAIL" "$PGADMIN_PASSWORD")
else
  require STAGING_BASIC_AUTH_USER STAGING_BASIC_AUTH_PASSWORD

  apply_secret generic staging-basic-auth \
    --from-literal=users="$STAGING_BASIC_AUTH_USER:$(openssl passwd -apr1 "$STAGING_BASIC_AUTH_PASSWORD")"
fi

# ── Empreintes pour les annotations checksum/* des modèles de pods ────────────
{
  echo "CHECKSUM_DB_PASSWORD=$SUM_DB"
  echo "CHECKSUM_API_SECRETS=$SUM_API"
  echo "CHECKSUM_PGADMIN_SECRETS=$SUM_PGADMIN"
} >> "${GITHUB_ENV:-/dev/stdout}"
