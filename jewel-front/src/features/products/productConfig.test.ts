import { describe, expect, it } from 'vitest';
import type { ChainDto, ChainLengthPriceDto, CharmDto } from '../../api/products';
import { formatPrice } from '../../lib/format';
import { PRODUCT_CONFIG } from './productConfig';

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
