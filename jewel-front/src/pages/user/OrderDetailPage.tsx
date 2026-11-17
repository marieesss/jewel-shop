import { useState } from 'react';
import { Link, useParams } from 'react-router-dom';
import { cancelMyOrder, getMyOrder, receiveMyOrder } from '../../api/orders';
import { extractApiError } from '../../api/errors';
import { Alert } from '../../components/ui/Alert';
import { Badge } from '../../components/ui/Badge';
import { PageHeader } from '../../components/ui/PageHeader';
import { Spinner } from '../../components/ui/Spinner';
import { OrderActions } from '../../features/orders/OrderActions';
import { OrderDeliveryCard } from '../../features/orders/OrderDeliveryCard';
import { OrderLinesCard } from '../../features/orders/OrderLinesCard';
import { OrderStatusHistory } from '../../features/orders/OrderStatusHistory';
import { orderPaths } from '../../features/orders/orderPaths';
import { useAsync } from '../../hooks/useAsync';
import { formatDate } from '../../lib/format';
import {
  CANCEL_SUCCESS_MESSAGE,
  RECEIVE_SUCCESS_MESSAGE,
  cancelConfirmationMessage,
  isOrderStatusConflict,
  orderStatusLabel,
} from '../../lib/orderStatus';

/** Détail d'une commande du client, avec l'annulation et la confirmation de réception. */
export function OrderDetailPage() {
  const { id } = useParams();
  const orderId = Number(id);
  const { data: order, loading, error, reload } = useAsync(() => getMyOrder(orderId), [orderId]);
  const [actionError, setActionError] = useState<string | null>(null);
  const [actionSuccess, setActionSuccess] = useState<string | null>(null);
  const [actionInProgress, setActionInProgress] = useState(false);

  const runAction = async (action: (id: number) => Promise<unknown>, successMessage: string) => {
    setActionError(null);
    setActionSuccess(null);
    setActionInProgress(true);
    try {
      await action(orderId);
      setActionSuccess(successMessage);
      reload();
    } catch (err) {
      setActionError(extractApiError(err));
      if (isOrderStatusConflict(err)) reload();
    } finally {
      setActionInProgress(false);
    }
  };

  const handleCancel = () => {
    if (!order || !window.confirm(cancelConfirmationMessage(order.number))) return;
    runAction(cancelMyOrder, CANCEL_SUCCESS_MESSAGE);
  };

  const handleReceive = () => runAction(receiveMyOrder, RECEIVE_SUCCESS_MESSAGE);

  return (
    <div className="mx-auto w-full max-w-[760px]">
      <Link
        to={orderPaths.list}
        className="mb-5 inline-flex items-center gap-1 font-body text-[13px] font-medium text-slate transition-colors hover:text-fuchsia"
      >
        ← Retour à mes commandes
      </Link>

      {actionError && <Alert className="mb-4">{actionError}</Alert>}
      {actionSuccess && (
        <Alert variant="success" className="mb-4">
          {actionSuccess}
        </Alert>
      )}

      {loading && <Spinner label="Chargement…" />}
      {!loading && error && <Alert>{error}</Alert>}

      {!loading && !error && order && (
        <>
          <PageHeader
            title={`Commande ${order.number}`}
            subtitle={`Passée le ${formatDate(order.createdAt)}`}
          />
          <div className="mb-6">
            <Badge>{orderStatusLabel(order.status)}</Badge>
          </div>

          <OrderActions
            status={order.status}
            disabled={actionInProgress}
            onCancel={handleCancel}
            onReceive={handleReceive}
          />

          <OrderLinesCard order={order} />
          <OrderDeliveryCard order={order} />
          <OrderStatusHistory history={order.statusHistory} />
        </>
      )}
    </div>
  );
}
