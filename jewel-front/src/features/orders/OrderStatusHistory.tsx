import type { OrderStatusChange } from '../../api/orders';
import { Card } from '../../components/ui/Card';
import { DetailRow } from '../../components/ui/DetailRow';
import { formatDate } from '../../lib/format';
import { orderStatusLabel } from '../../lib/orderStatus';

interface OrderStatusHistoryProps {
  history: OrderStatusChange[];
}

/** Historique des statuts, du plus ancien au plus récent (ordre de l'API). */
export function OrderStatusHistory({ history }: OrderStatusHistoryProps) {
  return (
    <Card className="mb-6">
      <h2 className="mb-4 font-comfortaa text-xl font-light text-storm">Historique</h2>
      {history.map((change, index) => (
        <DetailRow key={`${change.toStatus}-${index}`} label={formatDate(change.createdAt)}>
          {orderStatusLabel(change.toStatus)}
        </DetailRow>
      ))}
    </Card>
  );
}
