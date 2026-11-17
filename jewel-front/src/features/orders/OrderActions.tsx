import type { OrderStatus } from '../../api/orders';
import { Button } from '../../components/ui/Button';
import { canCancelOrder, canMarkOrderReceived } from '../../lib/orderStatus';

interface OrderActionsProps {
  status: OrderStatus;
  /** Une action est en cours : les boutons sont désactivés. */
  disabled: boolean;
  onCancel: () => void;
  onReceive: () => void;
}

/** Actions permises au client selon le statut ; rien n'est affiché si aucune ne l'est. */
export function OrderActions({ status, disabled, onCancel, onReceive }: OrderActionsProps) {
  const showCancel = canCancelOrder(status);
  const showReceive = canMarkOrderReceived(status);
  if (!showCancel && !showReceive) return null;

  return (
    <div className="mb-6 flex flex-wrap gap-3">
      {showCancel && (
        <Button variant="secondary" disabled={disabled} onClick={onCancel}>
          Annuler la commande
        </Button>
      )}
      {showReceive && (
        <Button disabled={disabled} onClick={onReceive}>
          J’ai reçu ma commande
        </Button>
      )}
    </div>
  );
}
