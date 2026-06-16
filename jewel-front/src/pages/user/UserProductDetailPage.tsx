import { Link, useParams } from 'react-router-dom';
import { Spinner } from '../../components/ui/Spinner';
import { Alert } from '../../components/ui/Alert';
import { Badge } from '../../components/ui/Badge';
import { Card } from '../../components/ui/Card';
import { DetailRow } from '../../components/ui/DetailRow';
import { useAsync } from '../../hooks/useAsync';
import { PRODUCT_CONFIG, userPaths } from '../../features/products/productConfig';
import { ProductImage } from '../../features/products/ProductImage';
import { FavoriteButton } from '../../features/favorites/FavoriteButton';
import { colorLabel, colorTone } from '../../api/products';
import { formatPrice } from '../../lib/format';
import type { ProductKind } from '../../stores/ProductAdminStore';

interface Props {
  kind: ProductKind;
}

/** Détail produit côté utilisateur, avec ajout/retrait des favoris. */
export function UserProductDetailPage({ kind }: Props) {
  const config = PRODUCT_CONFIG[kind];
  const { id } = useParams();
  const productId = Number(id);

  const { data, loading, error } = useAsync(
    () => config.fetchById(productId),
    [kind, productId]
  );

  return (
    <div className="mx-auto w-full max-w-[760px]">
      <Link
        to={userPaths.list(config.slug)}
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
              {formatPrice(data.price)}
            </p>

            <div className="mb-5">
              <FavoriteButton kind={kind} productId={data.id} variant="inline" />
            </div>

            {data.description && (
              <p className="mb-5 font-body text-sm leading-relaxed text-encre/80">
                {data.description}
              </p>
            )}

            <Card className="px-5 py-2">
              <DetailRow label="Couleur">{colorLabel(data.color)}</DetailRow>
              <DetailRow label={config.specificField(data).label}>
                {config.specificField(data).value}
              </DetailRow>
              <DetailRow label="Prix">{formatPrice(data.price)}</DetailRow>
              {data.url && (
                <DetailRow label="Lien">
                  <a
                    href={data.url}
                    target="_blank"
                    rel="noreferrer"
                    className="text-fuchsia underline underline-offset-2 hover:opacity-70"
                  >
                    Ouvrir
                  </a>
                </DetailRow>
              )}
            </Card>
          </div>
        </div>
      )}
    </div>
  );
}
