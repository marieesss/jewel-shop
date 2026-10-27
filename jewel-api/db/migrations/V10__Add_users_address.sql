-- Colonnes nullables : les comptes existants n'ont ni adresse ni téléphone.
-- Livraison en France uniquement : pas de colonne pays.
ALTER TABLE users ADD COLUMN IF NOT EXISTS phone VARCHAR(20) NULL;
ALTER TABLE users ADD COLUMN IF NOT EXISTS address_line1 VARCHAR(255) NULL;
ALTER TABLE users ADD COLUMN IF NOT EXISTS address_line2 VARCHAR(255) NULL;
ALTER TABLE users ADD COLUMN IF NOT EXISTS postal_code VARCHAR(5) NULL;
ALTER TABLE users ADD COLUMN IF NOT EXISTS city VARCHAR(100) NULL;
