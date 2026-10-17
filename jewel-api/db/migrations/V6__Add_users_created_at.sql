-- En deux instructions : la colonne est d'abord ajoutée sans défaut, pour que les comptes
-- existants gardent created_at à NULL (leur date d'inscription n'est pas connue).
ALTER TABLE users ADD COLUMN IF NOT EXISTS created_at TIMESTAMP NULL;
ALTER TABLE users ALTER COLUMN created_at SET DEFAULT NOW();
