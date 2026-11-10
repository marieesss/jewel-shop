import type { CreateCharmPayload } from '../../api/products';
import { toNumber, type ProductCommonValues } from './productForm';

/**
 * Corps de `POST /api/charms` et de `PUT /api/charms/{id}`. `tagIds` est toujours envoyé :
 * une liste vide retire tous les tags.
 */
export function toCharmPayload(common: ProductCommonValues, stock: number): CreateCharmPayload {
  return {
    name: common.name.trim(),
    description: common.description.trim(),
    color: common.color,
    url: common.url.trim() || null,
    cost: toNumber(common.cost),
    price: toNumber(common.price),
    stock,
    tagIds: common.tagIds,
  };
}
