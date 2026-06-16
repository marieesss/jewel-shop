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
import type { ProductKind } from '../../stores/ProductAdminStore';

export type ProductDto = CharmDto | ChainDto;
export type ProductSlug = 'charms' | 'chains';

interface SpecificField {
  label: string;
  value: string;
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
  specificField: (product: ProductDto) => SpecificField;
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
  },
  chain: {
    kind: 'chain',
    slug: 'chains',
    singular: 'Chaîne',
    plural: 'Chaînes',
    hasStock: false,
    fetchList: getChains,
    fetchById: getChainById,
    specificField: (p) => ({ label: 'Longueur', value: `${(p as ChainDto).length} cm` }),
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
