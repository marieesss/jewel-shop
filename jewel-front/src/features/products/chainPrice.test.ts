import { describe, expect, it } from 'vitest';
import type { ChainLengthPriceDto } from '../../api/products';
import { formatPrice } from '../../lib/format';
import {
  displayedChainPrice,
  findSelectedLength,
  formatFromPrice,
  formatLengthOption,
  isChainUnavailable,
  isSelectableLength,
  selectedLengthFor,
} from './chainPrice';

const length = (
  lengthCm: number,
  price: number | null,
  available = true,
  label = 'Princesse'
): ChainLengthPriceDto => ({ lengthCm, label, price, available });

const chain = {
  availableLengths: [length(38, 12.5, false), length(45, 14.2), length(50, 15.4)],
  fromPrice: 14.2,
};

describe('formatFromPrice', () => {
  it('affiche « à partir de » le prix avec 2 décimales et le symbole €', () => {
    expect(formatFromPrice(14.2)).toBe(`à partir de ${formatPrice(14.2)}`);
  });

  it('affiche « à partir de » un prix nul', () => {
    expect(formatFromPrice(0)).toBe(`à partir de ${formatPrice(0)}`);
  });

  it('affiche « Indisponible » sans prix « à partir de »', () => {
    expect(formatFromPrice(null)).toBe('Indisponible');
    expect(formatFromPrice(undefined)).toBe('Indisponible');
  });
});

describe('isChainUnavailable', () => {
  it('considère disponible une chaîne qui a un prix « à partir de »', () => {
    expect(isChainUnavailable(chain)).toBe(false);
  });

  it('considère indisponible une chaîne sans prix « à partir de »', () => {
    expect(
      isChainUnavailable({ availableLengths: [length(45, 14.2, false)], fromPrice: null })
    ).toBe(true);
  });
});

describe('formatLengthOption', () => {
  it('affiche le libellé et la longueur en cm', () => {
    expect(formatLengthOption(length(45, 14.2))).toBe('Princesse – 45 cm');
  });
});

describe('isSelectableLength', () => {
  it('accepte une longueur disponible avec un prix', () => {
    expect(isSelectableLength(length(45, 14.2))).toBe(true);
  });

  it('refuse une longueur indisponible', () => {
    expect(isSelectableLength(length(45, 14.2, false))).toBe(false);
  });

  it('refuse une longueur sans prix', () => {
    expect(isSelectableLength(length(45, null))).toBe(false);
  });
});

describe('findSelectedLength', () => {
  it('renvoie la longueur choisie quand elle est disponible', () => {
    expect(findSelectedLength(chain, 45)).toEqual(length(45, 14.2));
  });

  it('ignore une longueur indisponible', () => {
    expect(findSelectedLength(chain, 38)).toBeNull();
  });

  it('ignore une longueur inconnue ou l’absence de choix', () => {
    expect(findSelectedLength(chain, 99)).toBeNull();
    expect(findSelectedLength(chain, null)).toBeNull();
  });

  it('accepte une chaîne sans liste de longueurs', () => {
    expect(findSelectedLength({ availableLengths: null, fromPrice: null }, 45)).toBeNull();
  });
});

describe('displayedChainPrice', () => {
  it('affiche « à partir de » sans longueur choisie', () => {
    expect(displayedChainPrice(chain, null)).toBe(`à partir de ${formatPrice(14.2)}`);
  });

  it('affiche le prix de 45 cm quand le client choisit 45 cm', () => {
    expect(displayedChainPrice(chain, 45)).toBe(formatPrice(14.2));
  });

  it('affiche le prix de la nouvelle longueur quand le client change de choix', () => {
    expect(displayedChainPrice(chain, 50)).toBe(formatPrice(15.4));
  });

  it('garde « à partir de » si la longueur choisie est indisponible', () => {
    expect(displayedChainPrice(chain, 38)).toBe(`à partir de ${formatPrice(14.2)}`);
  });

  it('affiche « Indisponible » pour une chaîne sans longueur disponible', () => {
    expect(
      displayedChainPrice({ availableLengths: [length(45, 14.2, false)], fromPrice: null }, null)
    ).toBe('Indisponible');
  });
});

describe('selectedLengthFor', () => {
  it('renvoie la longueur choisie sur la fiche du même produit', () => {
    expect(selectedLengthFor({ productId: 3, lengthCm: 45 }, 3)).toBe(45);
  });

  it("n'applique pas le choix à la fiche d'un autre produit", () => {
    expect(selectedLengthFor({ productId: 3, lengthCm: 45 }, 4)).toBeNull();
  });

  it('renvoie null sans choix', () => {
    expect(selectedLengthFor(null, 3)).toBeNull();
  });
});
