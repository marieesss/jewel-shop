CREATE TABLE IF NOT EXISTS email_outbox (
    id              BIGSERIAL    PRIMARY KEY,
    to_address      VARCHAR(255) NOT NULL,
    subject         VARCHAR(255) NOT NULL,
    html_body       TEXT         NOT NULL,
    text_body       TEXT         NOT NULL,
    status          VARCHAR(20)  NOT NULL DEFAULT 'Pending',
    attempts        INT          NOT NULL DEFAULT 0,
    next_attempt_at TIMESTAMP    NOT NULL DEFAULT NOW(),
    last_error      TEXT,
    created_at      TIMESTAMP    NOT NULL DEFAULT NOW(),
    sent_at         TIMESTAMP
);

CREATE INDEX IF NOT EXISTS ix_email_outbox_status_next_attempt_at ON email_outbox (status, next_attempt_at);
