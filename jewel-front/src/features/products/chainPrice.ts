import type { ChainDto, ChainLengthPriceDto } from '../../api/products';
import { formatPrice } from '../../lib/format';

export const CHAIN_UNAVAILABLE_LABEL = 'Indisponible';
export const LENGTH_UNAVAILABLE_LABEL = 'indisponible';

type ChainPricing = Pick<ChainDto, 'availableLengths' | 'fromPrice'>;
type SelectableLength = ChainLengthPriceDto & { price: number };

/** « à partir de 12,50 € », ou « Indisponible » quand aucune longueur n'est disponible. */
export function formatFromPrice(fromPrice: number | null | undefined): string {
  return isPriced(fromPrice) ? `à partir de ${formatPrice(fromPrice)}` : CHAIN_UNAVAILABLE_LABEL;
}

/** Une chaîne sans aucune longueur disponible n'a pas de prix « à partir de ». */
export function isChainUnavailable(chain: ChainPricing): boolean {
  return !isPriced(chain.fromPrice);
}

/** Libellé d'une longueur dans le sélecteur (ex. « Princesse – 45 cm »). */
export function formatLengthOption(length: ChainLengthPriceDto): string {
  return `${length.label} – ${length.lengthCm} cm`;
}

/** Une longueur se choisit si elle est disponible et a un prix. */
export function isSelectableLength(length: ChainLengthPriceDto): length is SelectableLength {
  return length.available && isPriced(length.price);
}

/** Longueur choisie par le client, si elle existe et peut être choisie. */
export function findSelectedLength(
  chain: ChainPricing,
  selectedLengthCm: number | null
): SelectableLength | null {
  const length = (chain.availableLengths ?? []).find((l) => l.lengthCm === selectedLengthCm);
  return length && isSelectableLength(length) ? length : null;
}

/** Prix affiché sur la fiche : celui de la longueur choisie, sinon « à partir de ». */
export function displayedChainPrice(chain: ChainPricing, selectedLengthCm: number | null): string {
  const selected = findSelectedLength(chain, selectedLengthCm);
  return selected ? formatPrice(selected.price) : formatFromPrice(chain.fromPrice);
}

function isPriced(price: number | null | undefined): price is number {
  return price !== null && price !== undefined;
}
