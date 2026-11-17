import { AxiosError, AxiosHeaders, type InternalAxiosRequestConfig } from 'axios';
import { describe, expect, it } from 'vitest';
import { ORDER_STATUSES, type OrderLine } from '../api/orders';
import {
  canCancelOrder,
  canMarkOrderReceived,
  cancelConfirmationMessage,
  isOrderStatusConflict,
  orderLineLabel,
  orderStatusLabel,
} from './orderStatus';

function httpError(status: number): AxiosError {
  const config = { headers: new AxiosHeaders() } as InternalAxiosRequestConfig;
  return new AxiosError('Erreur', 'ERR_BAD_REQUEST', config, null, {
    status,
    statusText: '',
    data: { status, title: 'Cette commande ne peut plus être annulée.' },
    headers: {},
    config,
  });
}

const baseLine: OrderLine = {
  kind: 'Charm',
  productId: 1,
  name: 'Coquillage',
  lengthCm: null,
  position: null,
  pricePerMeter: null,
  claspPrice: null,
  unitPrice: 5,
};

describe('orderStatusLabel', () => {
  it.each([
    ['Pending', 'Validation'],
    ['Processing', 'Traitement'],
    ['Shipped', 'Envoyé'],
    ['Received', 'Reçu'],
    ['Cancelled', 'Annulée'],
  ] as const)('affiche le statut %s en « %s »', (status, label) => {
    expect(orderStatusLabel(status)).toBe(label);
  });
});

describe('canCancelOrder', () => {
  it('autorise l’annulation d’une commande en validation', () => {
    expect(canCancelOrder('Pending')).toBe(true);
  });

  it.each(ORDER_STATUSES.filter((status) => status !== 'Pending'))(
    'refuse l’annulation au statut %s',
    (status) => {
      expect(canCancelOrder(status)).toBe(false);
    }
  );
});

describe('canMarkOrderReceived', () => {
  it('autorise la réception d’une commande envoyée', () => {
    expect(canMarkOrderReceived('Shipped')).toBe(true);
  });

  it.each(ORDER_STATUSES.filter((status) => status !== 'Shipped'))(
    'refuse la réception au statut %s',
    (status) => {
      expect(canMarkOrderReceived(status)).toBe(false);
    }
  );
});

describe('Commande en traitement', () => {
  it('ne propose aucune action', () => {
    expect(canCancelOrder('Processing')).toBe(false);
    expect(canMarkOrderReceived('Processing')).toBe(false);
  });
});

describe('cancelConfirmationMessage', () => {
  it('nomme la commande à annuler', () => {
    expect(cancelConfirmationMessage('CMD-2026-00042')).toBe(
      'Annuler la commande CMD-2026-00042 ?'
    );
  });
});

describe('isOrderStatusConflict', () => {
  it('reconnaît un 409', () => {
    expect(isOrderStatusConflict(httpError(409))).toBe(true);
  });

  it.each([400, 404, 500])('ignore un %s', (status) => {
    expect(isOrderStatusConflict(httpError(status))).toBe(false);
  });

  it('ignore une erreur qui ne vient pas de l’API', () => {
    expect(isOrderStatusConflict(new Error('réseau'))).toBe(false);
  });
});

describe('orderLineLabel', () => {
  it('affiche la chaîne avec sa longueur', () => {
    const line: OrderLine = { ...baseLine, kind: 'Chain', name: 'Forçat doré', lengthCm: 45 };
    expect(orderLineLabel(line)).toBe('Forçat doré (45 cm)');
  });

  it('affiche la breloque avec sa position', () => {
    expect(orderLineLabel({ ...baseLine, position: 2 })).toBe('Breloque 2 : Coquillage');
  });
});
