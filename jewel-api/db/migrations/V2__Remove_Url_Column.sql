-- =============================================================================
-- V2 — Suppression colonne url (remplacée par image_url géré par Cloudinary)
-- =============================================================================

ALTER TABLE chains DROP COLUMN IF EXISTS url;
ALTER TABLE charms DROP COLUMN IF EXISTS url;
