import { describe, expect, it } from 'vitest';
import type { TagDto } from '../../api/tags';
import {
  TAG_NAME_REQUIRED_MESSAGE,
  TAG_NAME_TOO_LONG_MESSAGE,
  TAG_NAME_WITHOUT_LETTER_OR_DIGIT_MESSAGE,
  categoriesAvailableFor,
  groupTagsByCategory,
  selectedTags,
  tagSlug,
  toggleTagId,
  validateTagName,
  withTag,
} from './tagSelection';

const tag = (id: number, name: string, category: string): TagDto => ({
  id,
  name,
  slug: tagSlug(name),
  category,
});

const plage = tag(1, 'Plage', 'Theme');
const mer = tag(2, 'Mer', 'Theme');
const dore = tag(3, 'Doré', 'Color');
const mariage = tag(4, 'Mariage', 'Occasion');
const acier = tag(5, 'Acier', 'Material');
const tags = [mariage, plage, dore, mer, acier];

describe('groupTagsByCategory', () => {
  it("groupe les tags par catégorie, dans l'ordre de l'API, triés par nom", () => {
    expect(groupTagsByCategory(tags, '')).toEqual([
      { category: 'Theme', label: 'Thème', tags: [mer, plage] },
      { category: 'Color', label: 'Couleur', tags: [dore] },
      { category: 'Material', label: 'Matière', tags: [acier] },
      { category: 'Occasion', label: 'Occasion', tags: [mariage] },
    ]);
  });

  it('filtre sur la recherche sans tenir compte de la casse ni des accents', () => {
    expect(groupTagsByCategory(tags, ' DORE ')).toEqual([
      { category: 'Color', label: 'Couleur', tags: [dore] },
    ]);
  });

  it('ne renvoie aucun groupe quand aucun tag ne correspond', () => {
    expect(groupTagsByCategory(tags, 'licorne')).toEqual([]);
  });
});

describe('selectedTags', () => {
  it('renvoie les tags choisis triés par catégorie puis par nom', () => {
    expect(selectedTags(tags, [mariage.id, plage.id, mer.id])).toEqual([mer, plage, mariage]);
  });

  it('ignore un id absent de la liste', () => {
    expect(selectedTags(tags, [99])).toEqual([]);
  });
});

describe('toggleTagId', () => {
  it('ajoute un tag non sélectionné', () => {
    expect(toggleTagId([1], 3)).toEqual([1, 3]);
  });

  it('retire un tag déjà sélectionné', () => {
    expect(toggleTagId([1, 3], 1)).toEqual([3]);
  });
});

describe('withTag', () => {
  it('ajoute un tag créé à la liste', () => {
    const licorne = tag(6, 'Licorne', 'Theme');
    expect(withTag(tags, licorne)).toEqual([...tags, licorne]);
  });

  it('ne duplique pas un tag déjà présent', () => {
    expect(withTag(tags, plage)).toBe(tags);
  });
});

describe('categoriesAvailableFor', () => {
  it('propose toutes les catégories pour un nom inconnu', () => {
    expect(categoriesAvailableFor('Licorne', tags)).toEqual([
      'Theme',
      'Color',
      'Material',
      'Occasion',
    ]);
  });

  it('exclut les catégories où le nom existe déjà, sans tenir compte de la casse', () => {
    expect(categoriesAvailableFor('  doré ', tags)).toEqual(['Theme', 'Material', 'Occasion']);
  });

  it('ne propose rien pour une saisie vide', () => {
    expect(categoriesAvailableFor('   ', tags)).toEqual([]);
  });
});

describe('tagSlug', () => {
  it.each([
    ['Bord de mer', 'bord-de-mer'],
    ['  Œillet & Cœur  ', 'oeillet-coeur'],
    ['Ex æquo', 'ex-aequo'],
    ['Été 2026 !', 'ete-2026'],
  ])('calcule le slug de « %s »', (name, slug) => {
    expect(tagSlug(name)).toBe(slug);
  });

  it('limite le slug à 60 caractères sans tiret final', () => {
    const name = `${'a'.repeat(59)} b`;
    expect(tagSlug(name)).toBe('a'.repeat(59));
  });
});

describe('validateTagName', () => {
  it('accepte un nom valide', () => {
    expect(validateTagName('Bord de mer')).toBeUndefined();
  });

  it('exige un nom', () => {
    expect(validateTagName('   ')).toBe(TAG_NAME_REQUIRED_MESSAGE);
  });

  it('accepte 50 caractères et refuse au-delà', () => {
    expect(validateTagName('a'.repeat(50))).toBeUndefined();
    expect(validateTagName('a'.repeat(51))).toBe(TAG_NAME_TOO_LONG_MESSAGE);
  });

  it('refuse un nom sans lettre ni chiffre', () => {
    expect(validateTagName('—!?')).toBe(TAG_NAME_WITHOUT_LETTER_OR_DIGIT_MESSAGE);
  });
});
