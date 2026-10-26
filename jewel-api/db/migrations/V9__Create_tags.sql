CREATE TABLE IF NOT EXISTS tags (
    id         BIGSERIAL   PRIMARY KEY,
    name       VARCHAR(50) NOT NULL,
    slug       VARCHAR(60) NOT NULL,
    category   VARCHAR(20) NOT NULL CHECK (category IN ('Theme', 'Color', 'Material', 'Occasion')),
    created_at TIMESTAMPTZ NOT NULL DEFAULT now()
);

-- Un même nom peut exister dans deux catégories (« Doré » en couleur et en matière) :
-- le nom et le slug ne sont uniques qu'à l'intérieur d'une catégorie.
CREATE UNIQUE INDEX IF NOT EXISTS ux_tags_category_name ON tags (category, lower(name));
CREATE UNIQUE INDEX IF NOT EXISTS ux_tags_category_slug ON tags (category, slug);

CREATE TABLE IF NOT EXISTS chain_tags (
    chain_id BIGINT NOT NULL REFERENCES chains (id) ON DELETE CASCADE,
    tag_id   BIGINT NOT NULL REFERENCES tags (id) ON DELETE CASCADE,
    PRIMARY KEY (chain_id, tag_id)
);

CREATE TABLE IF NOT EXISTS charm_tags (
    charm_id BIGINT NOT NULL REFERENCES charms (id) ON DELETE CASCADE,
    tag_id   BIGINT NOT NULL REFERENCES tags (id) ON DELETE CASCADE,
    PRIMARY KEY (charm_id, tag_id)
);

CREATE INDEX IF NOT EXISTS ix_chain_tags_tag_id ON chain_tags (tag_id);
CREATE INDEX IF NOT EXISTS ix_charm_tags_tag_id ON charm_tags (tag_id);
