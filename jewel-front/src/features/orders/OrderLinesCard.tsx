import type { OrderDetail } from '../../api/orders';
import { Card } from '../../components/ui/Card';
import { DetailRow } from '../../components/ui/DetailRow';
import { formatPrice } from '../../lib/format';
import { orderLineLabel } from '../../lib/orderStatus';

interface OrderLinesCardProps {
  order: OrderDetail;
}

/** Articles de la commande dans l'ordre du collier, aux prix de la commande, total et commentaire. */
export function OrderLinesCard({ order }: OrderLinesCardProps) {
  return (
    <Card className="mb-6">
      <h2 className="mb-4 font-comfortaa text-xl font-light text-storm">Articles</h2>
      <ul>
        {order.lines.map((line) => (
          <li
            key={`${line.kind}-${line.position ?? 0}`}
            className="flex items-baseline justify-between gap-4 border-b border-lin py-2.5 font-body text-sm text-encre"
          >
            <span>{orderLineLabel(line)}</span>
            <span className="whitespace-nowrap">{formatPrice(line.unitPrice)}</span>
          </li>
        ))}
      </ul>
      <div className="flex items-baseline justify-between gap-4 pt-3 font-body text-sm font-semibold text-storm">
        <span>Total</span>
        <span className="whitespace-nowrap">{formatPrice(order.total)}</span>
      </div>
      {order.comment && (
        <div className="mt-4">
          <DetailRow label="Commentaire">{order.comment}</DetailRow>
        </div>
      )}
    </Card>
  );
}
