import { describe, expect, it } from 'vitest';
import type { TagCountDto } from '../../api/tags';
import {
  TAGS_PARAM,
  currentPage,
  parseTagSlugs,
  serializeTagSlugs,
  tagFilterLabel,
  toggleTagSlug,
  visibleTagGroups,
  withTagSlugs,
} from './tagFilters';

const tag = (
  id: number,
  name: string,
  slug: string,
  category: string,
  count: number
): TagCountDto => ({ id, name, slug, category, count });

const mer = tag(1, 'Mer', 'mer', 'Theme', 2);
const plage = tag(2, 'Plage', 'plage', 'Theme', 1);
const dore = tag(3, 'Doré', 'dore', 'Color', 1);
const acier = tag(4, 'Acier', 'acier', 'Material', 3);

describe('parseTagSlugs', () => {
  it('renvoie une liste vide quand le paramètre est absent', () => {
    expect(parseTagSlugs(null)).toEqual([]);
  });

  it('renvoie une liste vide quand le paramètre est vide', () => {
    expect(parseTagSlugs('')).toEqual([]);
  });

  it("découpe les slugs sur la virgule, dans l'ordre de l'adresse", () => {
    expect(parseTagSlugs('mer,dore')).toEqual(['mer', 'dore']);
  });

  it('ignore les espaces, les valeurs vides et les doublons', () => {
    expect(parseTagSlugs(' mer , ,dore,mer,')).toEqual(['mer', 'dore']);
  });
});

describe('serializeTagSlugs', () => {
  it("ne renvoie rien quand aucun tag n'est coché", () => {
    expect(serializeTagSlugs([])).toBeUndefined();
  });

  it('joint les slugs par une virgule', () => {
    expect(serializeTagSlugs(['mer', 'dore'])).toBe('mer,dore');
  });
});

describe('withTagSlugs', () => {
  it('ajoute le paramètre tags', () => {
    expect(withTagSlugs(new URLSearchParams(), ['mer']).toString()).toBe('tags=mer');
  });

  it('remplace la valeur existante', () => {
    const params = new URLSearchParams('tags=mer');
    expect(withTagSlugs(params, ['mer', 'dore']).get(TAGS_PARAM)).toBe('mer,dore');
  });

  it("retire le paramètre quand plus aucun tag n'est coché", () => {
    const params = new URLSearchParams('tags=mer');
    expect(withTagSlugs(params, []).has(TAGS_PARAM)).toBe(false);
  });

  it("conserve les autres paramètres de l'adresse", () => {
    const params = new URLSearchParams('foo=1&tags=mer');
    expect(withTagSlugs(params, []).toString()).toBe('foo=1');
  });

  it("ne modifie pas l'objet reçu", () => {
    const params = new URLSearchParams('tags=mer');
    withTagSlugs(params, ['dore']);
    expect(params.get(TAGS_PARAM)).toBe('mer');
  });
});

describe('toggleTagSlug', () => {
  it("ajoute en fin de liste un slug qui n'est pas coché", () => {
    expect(toggleTagSlug(['mer'], 'dore')).toEqual(['mer', 'dore']);
  });

  it('retire un slug déjà coché', () => {
    expect(toggleTagSlug(['mer', 'dore'], 'mer')).toEqual(['dore']);
  });

  it("ne modifie pas la liste d'origine", () => {
    const slugs = ['mer'];
    toggleTagSlug(slugs, 'dore');
    expect(slugs).toEqual(['mer']);
  });
});

describe('currentPage', () => {
  it('garde la page mémorisée quand les tags sont les mêmes', () => {
    expect(currentPage({ page: 3, tagsParam: 'mer' }, 'mer')).toBe(3);
  });

  it('revient en page 1 quand les tags ont changé', () => {
    expect(currentPage({ page: 3, tagsParam: 'mer' }, 'mer,dore')).toBe(1);
  });

  it('revient en page 1 quand un premier tag est coché', () => {
    expect(currentPage({ page: 2, tagsParam: null }, 'mer')).toBe(1);
  });
});

describe('visibleTagGroups', () => {
  it("range les catégories dans leur ordre même si l'API les mélange", () => {
    expect(visibleTagGroups([acier, dore, mer])).toEqual([
      { category: 'Theme', label: 'Thème', tags: [mer] },
      { category: 'Color', label: 'Couleur', tags: [dore] },
      { category: 'Material', label: 'Matière', tags: [acier] },
    ]);
  });

  it("garde l'ordre des tags reçu de l'API dans une catégorie", () => {
    expect(visibleTagGroups([plage, mer])[0].tags).toEqual([plage, mer]);
  });

  it("n'affiche pas une catégorie sans tag", () => {
    const categories = visibleTagGroups([mer, dore]).map((group) => group.category);
    expect(categories).not.toContain('Occasion');
  });

  it("n'affiche pas un tag d'une catégorie inconnue", () => {
    const inconnu = tag(9, 'Autre', 'autre', 'Inconnue', 5);
    expect(visibleTagGroups([inconnu, mer])).toEqual([
      { category: 'Theme', label: 'Thème', tags: [mer] },
    ]);
  });
});

describe('tagFilterLabel', () => {
  it("affiche le nom du tag suivi de son nombre d'articles", () => {
    expect(tagFilterLabel({ ...mer, count: 10 })).toBe('Mer (10)');
  });
});
