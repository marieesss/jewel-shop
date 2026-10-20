import { describe, expect, it } from 'vitest';
import type { ChainDto } from '../../api/products';
import { formatPrice } from '../../lib/format';
import { PRODUCT_CONFIG } from './productConfig';

const newFormatChain: ChainDto = {
  id: 3,
  name: 'Forçat',
  description: '',
  color: 'Gold',
  imageUrl: null,
  url: null,
  cost: null,
  price: null,
  length: null,
  costPerMeter: 4.5,
  pricePerMeter: 20,
  claspCost: 0.8,
  claspPrice: 3,
  stockCm: 1000,
};

describe('fiche admin d’une chaîne', () => {
  it('affiche le stock total, les prix au mètre et le fermoir', () => {
    expect(PRODUCT_CONFIG.chain.adminFields(newFormatChain)).toEqual([
      { label: 'Stock total', value: '1000 cm' },
      { label: 'Coût au mètre', value: formatPrice(4.5) },
      { label: 'Prix au mètre', value: formatPrice(20) },
      { label: 'Coût du fermoir', value: formatPrice(0.8) },
      { label: 'Prix du fermoir', value: formatPrice(3) },
    ]);
  });

  it('affiche « — » pour un prix au mètre absent', () => {
    const fields = PRODUCT_CONFIG.chain.adminFields({ ...newFormatChain, pricePerMeter: null });
    expect(fields.find((f) => f.label === 'Prix au mètre')?.value).toBe('—');
  });
});

describe('fiche client d’une chaîne', () => {
  it('affiche « — » pour une chaîne sans longueur', () => {
    expect(PRODUCT_CONFIG.chain.specificField(newFormatChain)).toEqual({
      label: 'Longueur',
      value: '—',
    });
  });
});
