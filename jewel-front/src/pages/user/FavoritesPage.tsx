import { observer } from 'mobx-react-lite';
import { Link } from 'react-router-dom';
import { PageHeader } from '../../components/ui/PageHeader';
import { Spinner } from '../../components/ui/Spinner';
import { Alert } from '../../components/ui/Alert';
import { EmptyState } from '../../components/ui/EmptyState';
import { Button } from '../../components/ui/Button';
import { ProductCard } from '../../features/products/ProductCard';
import { PRODUCT_CONFIG, userPaths } from '../../features/products/productConfig';
import { useFavoritesStore } from '../../stores/context';
import type { CharmDto, ChainDto } from '../../api/products';
import type { ProductKind } from '../../stores/ProductAdminStore';

type FavItem = { kind: ProductKind; product: CharmDto | ChainDto };

/**
 * Les chaînes d'un favori n'ont pas leurs prix par longueur (`fromPrice` à `null`) :
 * leur carte n'affiche pas de prix plutôt qu'un « Indisponible » faux.
 */
function favoritePriceLabel(kind: ProductKind, product: CharmDto | ChainDto): string | undefined {
  return kind === 'charm' ? PRODUCT_CONFIG.charm.cardPrice(product).label : undefined;
}

/** Liste des favoris de l'utilisateur. */
export const FavoritesPage = observer(function FavoritesPage() {
  const favorites = useFavoritesStore();

  // Normalise chaque favori en { kind, produit } exploitable par ProductCard.
  const items = favorites.favorites.flatMap((f): FavItem[] => {
    if (f.charm) return [{ kind: 'charm', product: f.charm }];
    if (f.chain) return [{ kind: 'chain', product: f.chain }];
    return [];
  });

  return (
    <div className="mx-auto w-full max-w-[920px]">
      <PageHeader title="Mes favoris" subtitle="Vos coups de cœur" />

      {favorites.loading && !favorites.loaded && <Spinner label="Chargement…" />}
      {favorites.error && <Alert className="mb-4">{favorites.error}</Alert>}

      {favorites.loaded && items.length === 0 && (
        <EmptyState
          title="Aucun favori"
          description="Parcourez le catalogue et ajoutez vos pièces préférées."
          action={
            <Link to="/charms">
              <Button>Voir les breloques</Button>
            </Link>
          }
        />
      )}

      {items.length > 0 && (
        <div className="grid grid-cols-2 gap-4 sm:grid-cols-3 lg:grid-cols-4">
          {items.map(({ kind, product }) => (
            <ProductCard
              key={`${kind}-${product.id}`}
              to={userPaths.detail(kind === 'charm' ? 'charms' : 'chains', product.id)}
              name={product.name}
              color={product.color}
              priceLabel={favoritePriceLabel(kind, product)}
              imageUrl={product.imageUrl}
              favorite={{ kind, productId: product.id }}
            />
          ))}
        </div>
      )}
    </div>
  );
});
