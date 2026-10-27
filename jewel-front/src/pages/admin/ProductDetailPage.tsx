import { Link, useParams } from 'react-router-dom';
import { Spinner } from '../../components/ui/Spinner';
import { Alert } from '../../components/ui/Alert';
import { Badge } from '../../components/ui/Badge';
import { Button } from '../../components/ui/Button';
import { Card } from '../../components/ui/Card';
import { DetailRow } from '../../components/ui/DetailRow';
import { useAsync } from '../../hooks/useAsync';
import { PRODUCT_CONFIG, adminPaths } from '../../features/products/productConfig';
import { ProductImage } from '../../features/products/ProductImage';
import { colorLabel, colorTone } from '../../api/products';
import { MISSING_VALUE } from '../../lib/format';
import type { ProductKind } from '../../stores/ProductAdminStore';

interface ProductDetailPageProps {
  kind: ProductKind;
}

/** Détail d'un produit (breloque ou chaîne). */
export function ProductDetailPage({ kind }: ProductDetailPageProps) {
  const config = PRODUCT_CONFIG[kind];
  const { id } = useParams();
  const productId = Number(id);

  const { data, loading, error } = useAsync(() => config.fetchById(productId), [kind, productId]);

  return (
    <div className="mx-auto w-full max-w-[760px]">
      <Link
        to={adminPaths.list(config.slug)}
        className="mb-5 inline-flex items-center gap-1 font-body text-[13px] font-medium text-slate transition-colors hover:text-fuchsia"
      >
        ← Retour aux {config.plural.toLowerCase()}
      </Link>

      {loading && <Spinner label="Chargement…" />}
      {!loading && (error || Number.isNaN(productId)) && (
        <Alert>{error ?? 'Identifiant de produit invalide.'}</Alert>
      )}

      {!loading && !error && data && (
        <div className="grid gap-6 md:grid-cols-[280px_1fr]">
          <ProductImage
            src={data.imageUrl}
            alt={data.name}
            className="h-[280px] w-full rounded-[20px] border border-lin"
          />

          <div>
            <div className="mb-3 flex items-center gap-3">
              <h1 className="font-comfortaa text-[28px] font-light text-storm">{data.name}</h1>
              <Badge tone={colorTone(data.color)}>{colorLabel(data.color)}</Badge>
            </div>

            <p className="mb-4 font-display text-xl font-medium text-fuchsia">
              {config.adminPrice(data)}
            </p>

            <Link to={adminPaths.edit(config.slug, data.id)} className="mb-5 inline-block">
              <Button variant="secondary" className="px-4 py-2">
                Modifier
              </Button>
            </Link>

            {data.description && (
              <p className="mb-5 font-body text-sm leading-relaxed text-encre/80">
                {data.description}
              </p>
            )}

            <Card className="px-5 py-2">
              <DetailRow label="Référence">#{data.id}</DetailRow>
              <DetailRow label="Couleur">{colorLabel(data.color)}</DetailRow>
              {config.adminFields(data).map((field) => (
                <DetailRow key={field.label} label={field.label}>
                  {field.value}
                </DetailRow>
              ))}
              <DetailRow label="Tags">
                {data.tags && data.tags.length > 0 ? (
                  <span className="inline-flex flex-wrap justify-end gap-1.5">
                    {data.tags.map((tag) => (
                      <Badge key={tag.id}>{tag.name}</Badge>
                    ))}
                  </span>
                ) : (
                  <span className="text-slate">{MISSING_VALUE}</span>
                )}
              </DetailRow>
              <DetailRow label="Lien">
                {data.url ? (
                  <a
                    href={data.url}
                    target="_blank"
                    rel="noreferrer"
                    className="text-fuchsia underline underline-offset-2 hover:opacity-70"
                  >
                    Ouvrir
                  </a>
                ) : (
                  <span className="text-slate">—</span>
                )}
              </DetailRow>
            </Card>
          </div>
        </div>
      )}
    </div>
  );
}
