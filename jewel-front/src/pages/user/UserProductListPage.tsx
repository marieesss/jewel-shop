import { useEffect, useState } from 'react';
import { PageHeader } from '../../components/ui/PageHeader';
import { Spinner } from '../../components/ui/Spinner';
import { Alert } from '../../components/ui/Alert';
import { EmptyState } from '../../components/ui/EmptyState';
import { Pagination } from '../../components/ui/Pagination';
import { useAsync } from '../../hooks/useAsync';
import { PRODUCT_CONFIG, userPaths } from '../../features/products/productConfig';
import { ProductCard } from '../../features/products/ProductCard';
import {
  ProductFilters,
  emptyFilters,
  type ProductFilterValues,
} from '../../features/products/ProductFilters';
import type { ProductListParams } from '../../api/products';
import type { ProductKind } from '../../stores/ProductAdminStore';

const PAGE_SIZE = 12;

interface Props {
  kind: ProductKind;
}

/** Catalogue côté utilisateur : filtres + grille cliquable + favoris. */
export function UserProductListPage({ kind }: Props) {
  const config = PRODUCT_CONFIG[kind];
  const [page, setPage] = useState(1);
  const [filters, setFilters] = useState<ProductFilterValues>(emptyFilters);

  // Repart proprement quand on change de type via la navbar.
  useEffect(() => {
    setPage(1);
    setFilters(emptyFilters);
  }, [kind]);

  const params: ProductListParams = {
    page,
    pageSize: PAGE_SIZE,
    color: filters.color || undefined,
    minPrice: filters.minPrice !== '' ? Number(filters.minPrice) : undefined,
    maxPrice: filters.maxPrice !== '' ? Number(filters.maxPrice) : undefined,
    inStockOnly: config.hasStock && filters.inStockOnly ? true : undefined,
  };

  const { data, loading, error } = useAsync(
    () => config.fetchList(params),
    [kind, page, filters.color, filters.minPrice, filters.maxPrice, filters.inStockOnly]
  );

  const patchFilters = (patch: Partial<ProductFilterValues>) => {
    setFilters((f) => ({ ...f, ...patch }));
    setPage(1);
  };

  const resetFilters = () => {
    setFilters(emptyFilters);
    setPage(1);
  };

  return (
    <div className="mx-auto w-full max-w-[1100px]">
      <PageHeader title={config.plural} subtitle="Trouvez la pièce parfaite" />

      <div className="flex flex-col gap-6 md:flex-row md:items-start">
        {/* Filtres latéraux */}
        <aside className="w-full shrink-0 md:sticky md:top-2 md:w-[240px]">
          <ProductFilters
            value={filters}
            onChange={patchFilters}
            onReset={resetFilters}
            showStock={config.hasStock}
          />
        </aside>

        {/* Résultats */}
        <div className="min-w-0 flex-1">
          {loading && <Spinner label="Chargement…" />}
          {!loading && error && <Alert>{error}</Alert>}

          {!loading && !error && data && data.items.length === 0 && (
            <EmptyState
              title="Aucun résultat"
              description="Aucun produit ne correspond à ces filtres."
            />
          )}

          {!loading && !error && data && data.items.length > 0 && (
            <>
              <div className="grid grid-cols-2 gap-4 lg:grid-cols-3">
                {data.items.map((product) => (
                  <ProductCard
                    key={product.id}
                    to={userPaths.detail(config.slug, product.id)}
                    name={product.name}
                    color={product.color}
                    price={product.price}
                    imageUrl={product.imageUrl}
                    favorite={{ kind, productId: product.id }}
                  />
                ))}
              </div>

              <Pagination
                page={data.page}
                totalPages={data.totalPages}
                hasPrevious={data.hasPrevious}
                hasNext={data.hasNext}
                onChange={setPage}
              />
            </>
          )}
        </div>
      </div>
    </div>
  );
}
