import { describe, expect, it } from 'vitest';
import { commonFromProduct, tagIdsOf } from './productForm';

const product = {
  name: 'Gourmette',
  description: 'Maille fine',
  color: 'Gold',
  cost: 4,
  price: 9.5,
};

describe('commonFromProduct', () => {
  it('reprend le lien fournisseur du produit', () => {
    const values = commonFromProduct({ ...product, url: 'https://fournisseur.example/gourmette' });

    expect(values.url).toBe('https://fournisseur.example/gourmette');
  });

  it('laisse le lien vide quand il est nul', () => {
    expect(commonFromProduct({ ...product, url: null }).url).toBe('');
  });

  it('laisse le lien vide quand la réponse ne le contient pas', () => {
    expect(commonFromProduct(product).url).toBe('');
  });
});

describe('tagIdsOf', () => {
  it('reprend les ids des tags du produit', () => {
    const tags = [
      { id: 1, name: 'Plage', slug: 'plage', category: 'Theme' },
      { id: 6, name: 'Doré', slug: 'dore', category: 'Color' },
    ];

    expect(commonFromProduct({ ...product, tags }).tagIds).toEqual([1, 6]);
  });

  it.each([null, undefined])('ne sélectionne aucun tag quand la réponse vaut %s', (tags) => {
    expect(tagIdsOf(tags)).toEqual([]);
  });
});
