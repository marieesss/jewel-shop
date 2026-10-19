-- Phase « expand » : les anciennes colonnes cost, price et length restent remplies par l'API
-- en production. Elles ne deviennent nullables que pour les chaînes créées au nouveau format,
-- et seront supprimées dans une migration ultérieure.
ALTER TABLE chains ADD COLUMN cost_per_meter  NUMERIC(10, 2) NULL;
ALTER TABLE chains ADD COLUMN price_per_meter NUMERIC(10, 2) NULL;
ALTER TABLE chains ADD COLUMN clasp_cost      NUMERIC(10, 2) NOT NULL DEFAULT 0;
ALTER TABLE chains ADD COLUMN clasp_price     NUMERIC(10, 2) NOT NULL DEFAULT 0;
ALTER TABLE chains ALTER COLUMN cost   DROP NOT NULL;
ALTER TABLE chains ALTER COLUMN price  DROP NOT NULL;
ALTER TABLE chains ALTER COLUMN length DROP NOT NULL;

CREATE TABLE chain_rolls (
    id                  BIGSERIAL   PRIMARY KEY,
    chain_id            BIGINT      NOT NULL REFERENCES chains(id) ON DELETE CASCADE,
    initial_length_cm   INT         NOT NULL CHECK (initial_length_cm > 0),
    remaining_length_cm INT         NOT NULL CHECK (remaining_length_cm >= 0),
    created_at          TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    CHECK (remaining_length_cm <= initial_length_cm)
);
CREATE INDEX ix_chain_rolls_chain_id ON chain_rolls(chain_id);

-- ROUND de PostgreSQL arrondit le demi vers l'extérieur, alors que l'API arrondit au pair
-- (Math.Round par défaut de .NET) : cette fonction de session reproduit l'arrondi de l'API,
-- pour qu'une chaîne reprise ait les mêmes prix au mètre que si elle était créée par l'API.
CREATE FUNCTION pg_temp.round_half_even(value NUMERIC, scale INT) RETURNS NUMERIC AS $$
    SELECT CASE
        WHEN ABS(value * 10::NUMERIC ^ scale - TRUNC(value * 10::NUMERIC ^ scale)) = 0.5
            THEN (TRUNC(value * 10::NUMERIC ^ scale) + MOD(TRUNC(value * 10::NUMERIC ^ scale), 2)) / 10::NUMERIC ^ scale
        ELSE ROUND(value, scale)
    END
$$ LANGUAGE SQL IMMUTABLE;

UPDATE chains
SET cost_per_meter  = pg_temp.round_half_even(cost * 100 / length, 2),
    price_per_meter = pg_temp.round_half_even(price * 100 / length, 2)
WHERE length > 0;

-- Chaque chaîne existante représente un seul rouleau de sa longueur.
INSERT INTO chain_rolls (chain_id, initial_length_cm, remaining_length_cm)
SELECT id, ROUND(length)::INT, ROUND(length)::INT
FROM chains
WHERE ROUND(length) > 0;
