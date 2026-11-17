import { AxiosError } from 'axios';
import type { OrderLine, OrderStatus } from '../api/orders';
import { formatLengthCm } from './format';

export const ORDER_STATUS_LABELS: Record<OrderStatus, string> = {
  Pending: 'Validation',
  Processing: 'Traitement',
  Shipped: 'Envoyé',
  Received: 'Reçu',
  Cancelled: 'Annulée',
};

// Reprend OrderStateMachine côté API : le client annule en validation et confirme la réception d'un envoi.
const CANCELLABLE_STATUS: OrderStatus = 'Pending';
const RECEIVABLE_STATUS: OrderStatus = 'Shipped';

const HTTP_CONFLICT = 409;

export const CANCEL_SUCCESS_MESSAGE = 'Votre commande a été annulée.';
export const RECEIVE_SUCCESS_MESSAGE = 'Merci, votre commande est marquée comme reçue.';

export function orderStatusLabel(status: OrderStatus): string {
  return ORDER_STATUS_LABELS[status];
}

export function canCancelOrder(status: OrderStatus): boolean {
  return status === CANCELLABLE_STATUS;
}

export function canMarkOrderReceived(status: OrderStatus): boolean {
  return status === RECEIVABLE_STATUS;
}

export function cancelConfirmationMessage(orderNumber: string): string {
  return `Annuler la commande ${orderNumber} ?`;
}

/** Un 409 signale que le statut a changé entre-temps : le détail affiché n'est plus à jour. */
export function isOrderStatusConflict(err: unknown): boolean {
  return err instanceof AxiosError && err.response?.status === HTTP_CONFLICT;
}

/** Libellé d'une ligne : la chaîne avec sa longueur, chaque breloque avec sa position. */
export function orderLineLabel(line: OrderLine): string {
  if (line.kind === 'Chain') return `${line.name} (${formatLengthCm(line.lengthCm)})`;
  return `Breloque ${line.position} : ${line.name}`;
}
