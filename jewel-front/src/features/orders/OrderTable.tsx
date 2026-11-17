import { Link } from 'react-router-dom';
import type { OrderSummary } from '../../api/orders';
import { Badge } from '../../components/ui/Badge';
import { formatDate, formatPrice } from '../../lib/format';
import { orderStatusLabel } from '../../lib/orderStatus';
import { orderPaths } from './orderPaths';

const COLUMNS = ['Numéro', 'Date', 'Statut', 'Total'];

interface OrderTableProps {
  orders: OrderSummary[];
}

/** Commandes du client, dans l'ordre de l'API (des plus récentes aux plus anciennes). */
export function OrderTable({ orders }: OrderTableProps) {
  return (
    <div className="overflow-x-auto rounded-[20px] border border-white/60 bg-white shadow-card">
      <table className="w-full font-body text-sm text-storm">
        <thead>
          <tr className="border-b border-mist text-left text-xs uppercase tracking-wide text-slate">
            {COLUMNS.map((column) => (
              <th key={column} scope="col" className="whitespace-nowrap px-4 py-3 font-semibold">
                {column}
              </th>
            ))}
          </tr>
        </thead>
        <tbody>
          {orders.map((order) => (
            <tr key={order.id} className="border-b border-mist/60 last:border-0">
              <td className="whitespace-nowrap px-4 py-3">
                <Link
                  to={orderPaths.detail(order.id)}
                  className="font-medium text-storm underline-offset-2 hover:text-fuchsia hover:underline"
                >
                  {order.number}
                </Link>
              </td>
              <td className="whitespace-nowrap px-4 py-3">{formatDate(order.createdAt)}</td>
              <td className="whitespace-nowrap px-4 py-3">
                <Badge>{orderStatusLabel(order.status)}</Badge>
              </td>
              <td className="whitespace-nowrap px-4 py-3">{formatPrice(order.total)}</td>
            </tr>
          ))}
        </tbody>
      </table>
    </div>
  );
}
