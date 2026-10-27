import { describe, expect, it } from 'vitest';
import type { ProductCommonValues } from './productForm';
import { toCharmPayload } from './charmFormValues';

const common: ProductCommonValues = {
  name: '  Cœur  ',
  description: ' Émail rose ',
  color: 'Silver',
  url: '   ',
  cost: '1,5',
  price: '6',
  tagIds: [3, 8],
};

describe('toCharmPayload', () => {
  it('envoie les champs nettoyés, le stock et les tags choisis', () => {
    expect(toCharmPayload(common, 12)).toEqual({
      name: 'Cœur',
      description: 'Émail rose',
      color: 'Silver',
      url: null,
      cost: 1.5,
      price: 6,
      stock: 12,
      tagIds: [3, 8],
    });
  });

  it("envoie une liste de tags vide quand aucun tag n'est choisi", () => {
    expect(toCharmPayload({ ...common, tagIds: [] }, 0).tagIds).toEqual([]);
  });
});
