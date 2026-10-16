CREATE TABLE IF NOT EXISTS password_reset_tokens (
    id          BIGSERIAL   PRIMARY KEY,
    user_id     BIGINT      NOT NULL REFERENCES users(id) ON DELETE CASCADE,
    token_hash  VARCHAR(64) NOT NULL UNIQUE,
    created_at  TIMESTAMP   NOT NULL DEFAULT NOW(),
    expires_at  TIMESTAMP   NOT NULL,
    used_at     TIMESTAMP
);

CREATE INDEX IF NOT EXISTS ix_password_reset_tokens_user_id ON password_reset_tokens (user_id);
