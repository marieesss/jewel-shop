import {
  getCharms,
  getChains,
  getCharmById,
  getChainById,
  type CharmDto,
  type ChainDto,
  type PagedResult,
  type PageParams,
} from '../../api/products';
import type { ProductKind } from '../../stores/ProductAdminStore';

export type ProductDto = CharmDto | ChainDto;

interface SpecificField {
  label: string;
  value: string;
}

export interface ProductKindConfig {
  kind: ProductKind;
  singular: string;
  plural: string;
  /** Base de route, ex. « /admin/products/charms ». */
  basePath: string;
  detailPath: (id: number) => string;
  fetchList: (params: PageParams) => Promise<PagedResult<ProductDto>>;
  fetchById: (id: number) => Promise<ProductDto>;
  /** Champ propre au type (stock pour la breloque, longueur pour la chaîne). */
  specificField: (product: ProductDto) => SpecificField;
}

const charmsBase = '/admin/products/charms';
const chainsBase = '/admin/products/chains';

export const PRODUCT_CONFIG: Record<ProductKind, ProductKindConfig> = {
  charm: {
    kind: 'charm',
    singular: 'Breloque',
    plural: 'Breloques',
    basePath: charmsBase,
    detailPath: (id) => `${charmsBase}/${id}`,
    fetchList: getCharms,
    fetchById: getCharmById,
    specificField: (p) => ({ label: 'Stock', value: String((p as CharmDto).stock) }),
  },
  chain: {
    kind: 'chain',
    singular: 'Chaîne',
    plural: 'Chaînes',
    basePath: chainsBase,
    detailPath: (id) => `${chainsBase}/${id}`,
    fetchList: getChains,
    fetchById: getChainById,
    specificField: (p) => ({ label: 'Longueur', value: `${(p as ChainDto).length} cm` }),
  },
};
