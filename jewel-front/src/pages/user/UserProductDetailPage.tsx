import { useState } from 'react';
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
import { FavoriteErrorAlert } from '../../features/favorites/FavoriteErrorAlert';
import { ChainLengthPicker } from '../../features/products/ChainLengthPicker';
import { selectedLengthFor, type LengthSelection } from '../../features/products/chainPrice';
import { colorLabel, colorTone, type ChainDto } from '../../api/products';
import type { ProductKind } from '../../stores/ProductAdminStore';

interface Props {
  kind: ProductKind;
}

/** Détail produit côté utilisateur, avec ajout/retrait des favoris. */
export function UserProductDetailPage({ kind }: Props) {
  const config = PRODUCT_CONFIG[kind];
  const { id } = useParams();
  const productId = Number(id);

  const { data, loading, error } = useAsync(() => config.fetchById(productId), [kind, productId]);
  // Le choix est rattaché au produit : passer à une autre fiche repart sans longueur choisie.
  const [selection, setSelection] = useState<LengthSelection | null>(null);
  const selectedLengthCm = selectedLengthFor(selection, productId);
  const isChain = kind === 'chain';

  const priceLabel = data && config.detailPrice(data, selectedLengthCm);

  return (
    <div className="mx-auto w-full max-w-[760px]">
      <Link
        to={userPaths.list(config.slug)}
        className="mb-5 inline-flex items-center gap-1 font-body text-[13px] font-medium text-slate transition-colors hover:text-fuchsia"
      >
        ← Retour aux {config.plural.toLowerCase()}
      </Link>

      <FavoriteErrorAlert className="mb-4" />

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

            <p className="mb-4 font-display text-xl font-medium text-fuchsia">{priceLabel}</p>

            {isChain && (
              <ChainLengthPicker
                lengths={(data as ChainDto).availableLengths ?? []}
                selectedLengthCm={selectedLengthCm}
                onSelect={(lengthCm) => setSelection({ productId, lengthCm })}
              />
            )}

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
              {!isChain && (
                <DetailRow label={config.specificField(data).label}>
                  {config.specificField(data).value}
                </DetailRow>
              )}
              <DetailRow label="Prix">{priceLabel}</DetailRow>
            </Card>
          </div>
        </div>
      )}
    </div>
  );
}
