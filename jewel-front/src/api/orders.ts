import { api } from './client';
import type { PagedResult } from './products';

export const ORDER_STATUSES = [
  'Pending',
  'Processing',
  'Shipped',
  'Received',
  'Cancelled',
] as const;
export type OrderStatus = (typeof ORDER_STATUSES)[number];

export type OrderLineKind = 'Chain' | 'Charm';

/** Commande dans la liste des commandes du client. */
export interface OrderSummary {
  id: number;
  number: string;
  /** Date UTC, sans fuseau. */
  createdAt: string;
  status: OrderStatus;
  total: number;
}

export interface OrderRecipient {
  name: string;
  surname: string;
  phone: string;
}

export interface OrderAddress {
  line1: string;
  line2: string | null;
  postalCode: string;
  city: string;
}

/** Ligne de la commande : la chaîne porte sa longueur, chaque breloque sa position (à partir de 1). */
export interface OrderLine {
  kind: OrderLineKind;
  productId: number;
  name: string;
  lengthCm: number | null;
  position: number | null;
  pricePerMeter: number | null;
  claspPrice: number | null;
  unitPrice: number;
}

/** Changement de statut. `fromStatus` est `null` pour la création de la commande. */
export interface OrderStatusChange {
  fromStatus: OrderStatus | null;
  toStatus: OrderStatus;
  createdAt: string;
}

/** Détail d'une commande : lignes dans l'ordre du collier, prix copiés à la commande. */
export interface OrderDetail {
  id: number;
  number: string;
  status: OrderStatus;
  comment: string | null;
  recipient: OrderRecipient;
  address: OrderAddress;
  lines: OrderLine[];
  total: number;
  statusHistory: OrderStatusChange[];
  createdAt: string;
  updatedAt: string;
}

/** GET /api/orders → PagedResult<OrderSummary>, des plus récentes aux plus anciennes */
export async function getMyOrders(
  page: number,
  pageSize: number
): Promise<PagedResult<OrderSummary>> {
  const { data } = await api.get<PagedResult<OrderSummary>>('/api/orders', {
    params: { page, pageSize },
  });
  return data;
}

/** GET /api/orders/{id} → détail d'une commande de l'utilisateur connecté */
export async function getMyOrder(id: number): Promise<OrderDetail> {
  const { data } = await api.get<OrderDetail>(`/api/orders/${id}`);
  return data;
}

/** POST /api/orders/{id}/cancel → 200 OrderDetail (409 si la commande n'est plus en validation) */
export async function cancelMyOrder(id: number): Promise<OrderDetail> {
  const { data } = await api.post<OrderDetail>(`/api/orders/${id}/cancel`);
  return data;
}

/** POST /api/orders/{id}/receive → 200 OrderDetail (409 si la commande n'est pas envoyée) */
export async function receiveMyOrder(id: number): Promise<OrderDetail> {
  const { data } = await api.post<OrderDetail>(`/api/orders/${id}/receive`);
  return data;
}
