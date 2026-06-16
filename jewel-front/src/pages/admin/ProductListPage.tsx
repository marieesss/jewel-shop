import { useEffect, useState } from 'react';
import { Link } from 'react-router-dom';
import { PageHeader } from '../../components/ui/PageHeader';
import { Spinner } from '../../components/ui/Spinner';
import { Alert } from '../../components/ui/Alert';
import { EmptyState } from '../../components/ui/EmptyState';
import { Button } from '../../components/ui/Button';
import { Pagination } from '../../components/ui/Pagination';
import { useAsync } from '../../hooks/useAsync';
import { PRODUCT_CONFIG, adminPaths } from '../../features/products/productConfig';
import { ProductCard } from '../../features/products/ProductCard';
import type { ProductKind } from '../../stores/ProductAdminStore';

const PAGE_SIZE = 12;

interface ProductListPageProps {
  kind: ProductKind;
}

/** Liste paginée des produits d'un type donné (breloques ou chaînes). */
export function ProductListPage({ kind }: ProductListPageProps) {
  const config = PRODUCT_CONFIG[kind];
  const [page, setPage] = useState(1);

  // Réinitialise la pagination quand on change de type via la navigation.
  useEffect(() => setPage(1), [kind]);

  const { data, loading, error } = useAsync(
    () => config.fetchList({ page, pageSize: PAGE_SIZE }),
    [kind, page]
  );

  return (
    <div className="mx-auto w-full max-w-[920px]">
      <PageHeader title={config.plural} subtitle="Catalogue des produits disponibles" />

      {loading && <Spinner label="Chargement…" />}
      {!loading && error && <Alert>{error}</Alert>}

      {!loading && !error && data && data.items.length === 0 && (
        <EmptyState
          title="Aucun produit"
          description={`Aucune ${config.singular.toLowerCase()} n'a encore été créée.`}
          action={
            <Link to="/admin/products/new">
              <Button>Créer un produit</Button>
            </Link>
          }
        />
      )}

      {!loading && !error && data && data.items.length > 0 && (
        <>
          <div className="grid grid-cols-2 gap-4 sm:grid-cols-3 lg:grid-cols-4">
            {data.items.map((product) => (
              <ProductCard
                key={product.id}
                to={adminPaths.detail(config.slug, product.id)}
                name={product.name}
                color={product.color}
                price={product.price}
                imageUrl={product.imageUrl}
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
  );
}
