-- Compteur unique des numéros de commande : il ne repart pas à 1 au changement d'année
-- (CMD-2026-0042 puis CMD-2027-0043).
CREATE SEQUENCE IF NOT EXISTS order_number_seq;

CREATE TABLE IF NOT EXISTS orders (
    id                BIGSERIAL      PRIMARY KEY,
    number            VARCHAR(20)    NOT NULL UNIQUE,
    user_id           BIGINT         NOT NULL REFERENCES users (id),
    status            VARCHAR(20)    NOT NULL CHECK (status IN ('Pending', 'Processing', 'Shipped', 'Received', 'Cancelled')),
    comment           VARCHAR(1000)  NULL,
    recipient_name    VARCHAR(100)   NOT NULL,
    recipient_surname VARCHAR(100)   NOT NULL,
    recipient_phone   VARCHAR(20)    NOT NULL,
    address_line1     VARCHAR(255)   NOT NULL,
    address_line2     VARCHAR(255)   NULL,
    postal_code       VARCHAR(5)     NOT NULL,
    city              VARCHAR(100)   NOT NULL,
    total             NUMERIC(10, 2) NOT NULL,
    created_at        TIMESTAMPTZ    NOT NULL DEFAULT now(),
    updated_at        TIMESTAMPTZ    NOT NULL DEFAULT now()
);
CREATE INDEX IF NOT EXISTS ix_orders_user_id ON orders (user_id);

-- Le nom et les prix sont copiés au moment de la commande : ils ne suivent pas le catalogue.
-- Les produits et les rouleaux référencés ne peuvent pas être supprimés (RESTRICT).
CREATE TABLE IF NOT EXISTS order_lines (
    id              BIGSERIAL      PRIMARY KEY,
    order_id        BIGINT         NOT NULL REFERENCES orders (id) ON DELETE CASCADE,
    kind            VARCHAR(10)    NOT NULL CHECK (kind IN ('Chain', 'Charm')),
    chain_id        BIGINT         NULL REFERENCES chains (id) ON DELETE RESTRICT,
    charm_id        BIGINT         NULL REFERENCES charms (id) ON DELETE RESTRICT,
    chain_roll_id   BIGINT         NULL REFERENCES chain_rolls (id) ON DELETE RESTRICT,
    name            VARCHAR(255)   NOT NULL,
    length_cm       INT            NULL,
    price_per_meter NUMERIC(10, 2) NULL,
    clasp_price     NUMERIC(10, 2) NULL,
    unit_price      NUMERIC(10, 2) NOT NULL,
    position        INT            NULL
);
CREATE INDEX IF NOT EXISTS ix_order_lines_order_id ON order_lines (order_id);
CREATE INDEX IF NOT EXISTS ix_order_lines_chain_id ON order_lines (chain_id);
CREATE INDEX IF NOT EXISTS ix_order_lines_charm_id ON order_lines (charm_id);
CREATE INDEX IF NOT EXISTS ix_order_lines_chain_roll_id ON order_lines (chain_roll_id);

CREATE TABLE IF NOT EXISTS order_status_history (
    id                BIGSERIAL     PRIMARY KEY,
    order_id          BIGINT        NOT NULL REFERENCES orders (id) ON DELETE CASCADE,
    from_status       VARCHAR(20)   NULL CHECK (from_status IN ('Pending', 'Processing', 'Shipped', 'Received', 'Cancelled')),
    to_status         VARCHAR(20)   NOT NULL CHECK (to_status IN ('Pending', 'Processing', 'Shipped', 'Received', 'Cancelled')),
    changed_by        BIGINT        NOT NULL REFERENCES users (id),
    message           VARCHAR(1000) NULL,
    customer_notified BOOLEAN       NOT NULL DEFAULT false,
    created_at        TIMESTAMPTZ   NOT NULL DEFAULT now()
);
CREATE INDEX IF NOT EXISTS ix_order_status_history_order_id ON order_status_history (order_id);

CREATE TABLE IF NOT EXISTS order_notes (
    id         BIGSERIAL     PRIMARY KEY,
    order_id   BIGINT        NOT NULL REFERENCES orders (id) ON DELETE CASCADE,
    author_id  BIGINT        NOT NULL REFERENCES users (id),
    content    VARCHAR(2000) NOT NULL,
    created_at TIMESTAMPTZ   NOT NULL DEFAULT now(),
    updated_at TIMESTAMPTZ   NOT NULL DEFAULT now()
);
CREATE INDEX IF NOT EXISTS ix_order_notes_order_id ON order_notes (order_id);
