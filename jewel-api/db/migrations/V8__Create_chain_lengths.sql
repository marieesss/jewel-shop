CREATE TABLE IF NOT EXISTS chain_lengths (
    id         BIGSERIAL   PRIMARY KEY,
    length_cm  INT         NOT NULL UNIQUE CHECK (length_cm BETWEEN 10 AND 200),
    label      VARCHAR(50) NOT NULL,
    created_at TIMESTAMPTZ NOT NULL DEFAULT now()
);

INSERT INTO chain_lengths (length_cm, label) VALUES
    (38, 'Ras du cou'),
    (42, 'Court'),
    (45, 'Princesse'),
    (50, 'Mi-long'),
    (55, 'Matinée'),
    (60, 'Long'),
    (65, 'Opéra court'),
    (70, 'Opéra'),
    (81, 'Sautoir')
ON CONFLICT (length_cm) DO NOTHING;
