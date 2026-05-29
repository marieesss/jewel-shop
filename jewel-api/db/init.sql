
CREATE TABLE users (
    id         BIGSERIAL PRIMARY KEY,
    name       VARCHAR(100) NOT NULL,
    surname    VARCHAR(100) NOT NULL,
    email      VARCHAR(255) NOT NULL UNIQUE,
    password_hash VARCHAR(255) NOT NULL,
    birthday   DATE,
    role       VARCHAR(20) NOT NULL DEFAULT 'User'
);

CREATE TABLE chains (
    id          BIGSERIAL PRIMARY KEY,
    name        VARCHAR(255) NOT NULL,
    description TEXT NOT NULL DEFAULT '',
    color       VARCHAR(20) NOT NULL,
    image_url   VARCHAR(500),
    cost        NUMERIC(10, 2) NOT NULL,
    price       NUMERIC(10, 2) NOT NULL,
    length      NUMERIC(6, 2) NOT NULL
);

CREATE TABLE charms (
    id          BIGSERIAL PRIMARY KEY,
    name        VARCHAR(255) NOT NULL,
    description TEXT NOT NULL DEFAULT '',
    color       VARCHAR(20) NOT NULL,
    image_url   VARCHAR(500),
    cost        NUMERIC(10, 2) NOT NULL,
    price       NUMERIC(10, 2) NOT NULL,
    stock       INT NOT NULL DEFAULT 0
);

CREATE TABLE options (
    id     BIGSERIAL PRIMARY KEY,
    name   VARCHAR(255) NOT NULL,
    length NUMERIC(6, 2) NOT NULL
);

CREATE TABLE favorites (
    id       BIGSERIAL PRIMARY KEY,
    user_id  BIGINT NOT NULL REFERENCES users(id) ON DELETE CASCADE,
    chain_id BIGINT REFERENCES chains(id) ON DELETE CASCADE,
    charm_id BIGINT REFERENCES charms(id) ON DELETE CASCADE,
    CONSTRAINT chk_favorites_one_product
        CHECK ((chain_id IS NOT NULL)::int + (charm_id IS NOT NULL)::int = 1),
    CONSTRAINT uq_favorites UNIQUE (user_id, chain_id, charm_id)
);

CREATE TABLE creations (
    id         BIGSERIAL PRIMARY KEY,
    user_id    BIGINT NOT NULL REFERENCES users(id) ON DELETE CASCADE,
    chain_id   BIGINT NOT NULL REFERENCES chains(id),
    option_id  BIGINT NOT NULL REFERENCES options(id),
    created_at TIMESTAMP NOT NULL DEFAULT NOW()
);

CREATE TABLE creation_items (
    id          BIGSERIAL PRIMARY KEY,
    creation_id BIGINT NOT NULL REFERENCES creations(id) ON DELETE CASCADE,
    charm_id    BIGINT NOT NULL REFERENCES charms(id),
    position    INT NOT NULL
);

-- Seed admin
INSERT INTO users (name, surname, email, password_hash, role)
VALUES (
    'Admin',
    'Admin',
    'admin@jewelryshop.com',
    -- bcrypt de 'Admin1234!' (BCrypt.Net-Next 4.0.3, workFactor=11)
    '$2a$11$EJFLqaz/lsaaFl1a5fR6DeV4ntS4O0XCKd4pYYp65MzE.UnMTVpZW',
    'Admin'
);
