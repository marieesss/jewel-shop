CREATE TABLE IF NOT EXISTS invitations (
    id               BIGSERIAL    PRIMARY KEY,
    email            VARCHAR(255) NOT NULL,
    token_hash       VARCHAR(64)  NOT NULL UNIQUE,
    status           VARCHAR(20)  NOT NULL DEFAULT 'Pending',
    expires_at       TIMESTAMP    NOT NULL,
    invited_by_id    BIGINT       REFERENCES users(id) ON DELETE SET NULL,
    created_at       TIMESTAMP    NOT NULL DEFAULT NOW(),
    accepted_at      TIMESTAMP,
    accepted_user_id BIGINT       REFERENCES users(id) ON DELETE SET NULL,
    revoked_at       TIMESTAMP
);

CREATE INDEX IF NOT EXISTS ix_invitations_email ON invitations (email);
CREATE INDEX IF NOT EXISTS ix_invitations_status ON invitations (status);
