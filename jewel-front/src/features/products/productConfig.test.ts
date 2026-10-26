import { describe, expect, it } from 'vitest';
import type { ChainDto, ChainLengthPriceDto, CharmDto } from '../../api/products';
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

describe('prix admin', () => {
  it('affiche le prix au mètre d’une chaîne au nouveau format, sans price', () => {
    expect(PRODUCT_CONFIG.chain.adminPrice(newFormatChain)).toBe(formatPrice(20));
  });

  it('affiche « — » pour une chaîne sans prix au mètre', () => {
    expect(PRODUCT_CONFIG.chain.adminPrice({ ...newFormatChain, pricePerMeter: null })).toBe('—');
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

const charm: CharmDto = {
  id: 1,
  name: 'Étoile',
  description: '',
  color: 'Gold',
  imageUrl: null,
  cost: 2,
  price: 5,
  stock: 3,
};

const length = (lengthCm: number, price: number): ChainLengthPriceDto => ({
  lengthCm,
  label: 'Princesse',
  price,
  available: true,
});

const chain = (overrides: Partial<ChainDto> = {}): ChainDto => ({
  id: 2,
  name: 'Maille',
  description: '',
  color: 'Silver',
  imageUrl: null,
  cost: 6,
  length: 40,
  price: 9,
  pricePerMeter: 20,
  claspPrice: 3,
  stockCm: 500,
  availableLengths: [length(45, 14.2), length(50, 15.4)],
  fromPrice: 14.2,
  ...overrides,
});

describe('PRODUCT_CONFIG.charm', () => {
  it('affiche le prix de la breloque sur sa carte', () => {
    expect(PRODUCT_CONFIG.charm.cardPrice(charm)).toEqual({
      label: formatPrice(5),
      unavailable: false,
    });
  });

  it('garde le prix de la breloque dans les favoris', () => {
    expect(PRODUCT_CONFIG.charm.favoritePrice(charm)).toEqual({
      label: formatPrice(5),
      unavailable: false,
    });
  });

  it('affiche le prix de la breloque sur sa fiche', () => {
    expect(PRODUCT_CONFIG.charm.detailPrice(charm, null)).toBe(formatPrice(5));
  });
});

describe('PRODUCT_CONFIG.chain', () => {
  it('affiche « à partir de » le plus petit prix sur la carte', () => {
    expect(PRODUCT_CONFIG.chain.cardPrice(chain())).toEqual({
      label: `à partir de ${formatPrice(14.2)}`,
      unavailable: false,
    });
  });

  it('grise la carte d’une chaîne dont fromPrice est null', () => {
    expect(PRODUCT_CONFIG.chain.cardPrice(chain({ fromPrice: null }))).toEqual({
      label: 'Indisponible',
      unavailable: true,
    });
  });

  it("n'affiche aucun prix pour une chaîne de favori", () => {
    const favoriteChain = chain({ fromPrice: null, availableLengths: null });
    expect(PRODUCT_CONFIG.chain.favoritePrice(favoriteChain)).toBeUndefined();
  });

  it('affiche le prix de la longueur choisie sur la fiche', () => {
    expect(PRODUCT_CONFIG.chain.detailPrice(chain(), 50)).toBe(formatPrice(15.4));
  });

  it('affiche « à partir de » sur la fiche sans longueur choisie', () => {
    expect(PRODUCT_CONFIG.chain.detailPrice(chain(), null)).toBe(
      `à partir de ${formatPrice(14.2)}`
    );
  });
});
