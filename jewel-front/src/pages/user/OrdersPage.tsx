import { useState } from 'react';
import { getMyOrders } from '../../api/orders';
import { Alert } from '../../components/ui/Alert';
import { EmptyState } from '../../components/ui/EmptyState';
import { PageHeader } from '../../components/ui/PageHeader';
import { Pagination } from '../../components/ui/Pagination';
import { Spinner } from '../../components/ui/Spinner';
import { OrderTable } from '../../features/orders/OrderTable';
import { useAsync } from '../../hooks/useAsync';

const PAGE_SIZE = 20;

/** « Mes commandes » : commandes de l'utilisateur connecté, paginées. */
export function OrdersPage() {
  const [page, setPage] = useState(1);
  const { data, loading, error } = useAsync(() => getMyOrders(page, PAGE_SIZE), [page]);

  return (
    <div className="mx-auto w-full max-w-[760px]">
      <PageHeader title="Mes commandes" subtitle="Suivez vos commandes et leur statut" />

      {loading && <Spinner label="Chargement…" />}
      {!loading && error && <Alert>{error}</Alert>}

      {!loading && !error && data && data.items.length === 0 && (
        <EmptyState title="Aucune commande." />
      )}

      {!loading && !error && data && data.items.length > 0 && (
        <>
          <OrderTable orders={data.items} />
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
