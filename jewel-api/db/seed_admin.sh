#!/bin/bash
set -e

export $(grep -v '^#' ../../.env | xargs)

# Insère en base
psql -h "$DB_HOST" -p "$DB_PORT" -U "$DB_USER" -d "$DB_NAME" -c "
INSERT INTO users (name, surname, email, password_hash, role)
SELECT '$ADMIN_NAME', '$ADMIN_SURNAME', '$ADMIN_EMAIL', '$ADMIN_PASSWORD', 'Admin'
WHERE NOT EXISTS (
    SELECT 1 FROM users WHERE email = '$ADMIN_EMAIL'
);"

echo "✓ Admin seedé avec succès"