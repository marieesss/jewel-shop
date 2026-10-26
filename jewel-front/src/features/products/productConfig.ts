import {
  getCharms,
  getChains,
  getCharmById,
  getChainById,
  type CharmDto,
  type ChainDto,
  type PagedResult,
  type ProductListParams,
} from '../../api/products';
import { displayedChainPrice, formatFromPrice, isChainUnavailable } from './chainPrice';
import type { ProductKind } from '../../stores/ProductAdminStore';
import { formatLengthCm, formatOptionalPrice, formatPrice } from '../../lib/format';

export type ProductDto = CharmDto | ChainDto;
export type ProductSlug = 'charms' | 'chains';

export interface DetailField {
  label: string;
  value: string;
}

export interface CardPrice {
  label: string;
  unavailable: boolean;
}

export interface ProductKindConfig {
  kind: ProductKind;
  slug: ProductSlug;
  singular: string;
  plural: string;
  /** Le type supporte-t-il le filtre « en stock » (breloques uniquement) ? */
  hasStock: boolean;
  fetchList: (params: ProductListParams) => Promise<PagedResult<ProductDto>>;
  fetchById: (id: number) => Promise<ProductDto>;
  /** Champ propre au type (stock pour la breloque, longueur pour la chaîne). */
  specificField: (product: ProductDto) => DetailField;
  /** Champs de la fiche admin propres au type (stock et prix). */
  adminFields: (product: ProductDto) => DetailField[];
  /** Prix affiché dans la liste admin et l'en-tête de la fiche admin (prix au mètre pour la chaîne). */
  adminPrice: (product: ProductDto) => string;
  /** Prix de la carte du catalogue client (« à partir de » pour la chaîne). */
  cardPrice: (product: ProductDto) => CardPrice;
  /**
   * Prix de la carte dans « Mes favoris ». Une chaîne de favori n'a pas ses prix par longueur :
   * sans prix, la carte n'affiche rien plutôt qu'un « Indisponible » faux.
   */
  favoritePrice: (product: ProductDto) => CardPrice | undefined;
  /** Prix de la fiche client, selon la longueur choisie (chaîne uniquement). */
  detailPrice: (product: ProductDto, selectedLengthCm: number | null) => string;
}

function charmAdminFields(charm: CharmDto): DetailField[] {
  return [
    { label: 'Stock', value: String(charm.stock) },
    { label: 'Coût', value: formatPrice(charm.cost) },
    { label: 'Prix', value: formatPrice(charm.price) },
  ];
}

function chainAdminFields(chain: ChainDto): DetailField[] {
  return [
    { label: 'Stock total', value: formatLengthCm(chain.stockCm) },
    { label: 'Coût au mètre', value: formatOptionalPrice(chain.costPerMeter) },
    { label: 'Prix au mètre', value: formatOptionalPrice(chain.pricePerMeter) },
    { label: 'Coût du fermoir', value: formatOptionalPrice(chain.claspCost) },
    { label: 'Prix du fermoir', value: formatPrice(chain.claspPrice) },
  ];
}

export const PRODUCT_CONFIG: Record<ProductKind, ProductKindConfig> = {
  charm: {
    kind: 'charm',
    slug: 'charms',
    singular: 'Breloque',
    plural: 'Breloques',
    hasStock: true,
    fetchList: getCharms,
    fetchById: getCharmById,
    specificField: (p) => ({ label: 'Stock', value: String((p as CharmDto).stock) }),
    adminFields: (p) => charmAdminFields(p as CharmDto),
    adminPrice: (p) => formatPrice((p as CharmDto).price),
    cardPrice: (p) => ({ label: formatPrice((p as CharmDto).price), unavailable: false }),
    favoritePrice: (p) => ({ label: formatPrice((p as CharmDto).price), unavailable: false }),
    detailPrice: (p) => formatPrice((p as CharmDto).price),
  },
  chain: {
    kind: 'chain',
    slug: 'chains',
    singular: 'Chaîne',
    plural: 'Chaînes',
    hasStock: false,
    fetchList: getChains,
    fetchById: getChainById,
    specificField: (p) => ({ label: 'Longueur', value: formatLengthCm((p as ChainDto).length) }),
    adminFields: (p) => chainAdminFields(p as ChainDto),
    adminPrice: (p) => formatOptionalPrice((p as ChainDto).pricePerMeter),
    cardPrice: (p) => ({
      label: formatFromPrice((p as ChainDto).fromPrice),
      unavailable: isChainUnavailable(p as ChainDto),
    }),
    favoritePrice: () => undefined,
    detailPrice: (p, selectedLengthCm) => displayedChainPrice(p as ChainDto, selectedLengthCm),
  },
};

/* ── Construction des chemins (une source unique par zone) ── */
export const adminPaths = {
  list: (slug: ProductSlug) => `/admin/products/${slug}`,
  detail: (slug: ProductSlug, id: number) => `/admin/products/${slug}/${id}`,
  edit: (slug: ProductSlug, id: number) => `/admin/products/${slug}/${id}/edit`,
};

export const userPaths = {
  list: (slug: ProductSlug) => `/${slug}`,
  detail: (slug: ProductSlug, id: number) => `/${slug}/${id}`,
};
