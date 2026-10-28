import { AxiosError, AxiosHeaders, type AxiosResponse } from 'axios';
import { describe, expect, it } from 'vitest';
import type { AdminTag, TagCategory } from '../api/tags';
import {
  NAME_REQUIRED_MESSAGE,
  NAME_TOO_LONG_MESSAGE,
  NAME_WITHOUT_LETTER_OR_DIGIT_MESSAGE,
  TAG_CATEGORY_OPTIONS,
  deleteConfirmationMessage,
  groupTagsByCategory,
  mergeConfirmationMessage,
  mergeTargetOptions,
  tagFormErrorsFromApi,
  tagUsageCount,
  validateTagName,
} from './tagAdmin';

function apiError(status: number, data: unknown): AxiosError {
  const response = {
    status,
    data,
    statusText: '',
    headers: {},
    config: { headers: new AxiosHeaders() },
  } as AxiosResponse;
  return new AxiosError('Erreur', undefined, undefined, undefined, response);
}

function tag(
  id: number,
  name: string,
  category: TagCategory,
  chainCount = 0,
  charmCount = 0
): AdminTag {
  return { id, name, slug: name.toLowerCase(), category, chainCount, charmCount };
}

describe('TAG_CATEGORY_OPTIONS', () => {
  it('propose les quatre catégories avec leur libellé français, dans l’ordre de l’API', () => {
    expect(TAG_CATEGORY_OPTIONS).toEqual([
      { value: 'Theme', label: 'Thème' },
      { value: 'Color', label: 'Couleur' },
      { value: 'Material', label: 'Matière' },
      { value: 'Occasion', label: 'Occasion' },
    ]);
  });
});

describe('groupTagsByCategory', () => {
  it('regroupe les tags sous Thème, Couleur, Matière et Occasion', () => {
    const mer = tag(1, 'Mer', 'Theme');
    const dore = tag(2, 'Doré', 'Color');
    const argent = tag(3, 'Argent', 'Material');
    const mariage = tag(4, 'Mariage', 'Occasion');
    const fleurs = tag(5, 'Fleurs', 'Theme');

    const groups = groupTagsByCategory([dore, mer, fleurs, argent, mariage]);

    expect(groups.map((group) => group.label)).toEqual(['Thème', 'Couleur', 'Matière', 'Occasion']);
    expect(groups.map((group) => group.tags)).toEqual([[mer, fleurs], [dore], [argent], [mariage]]);
  });

  it('garde une catégorie sans tag, avec une liste vide', () => {
    const groups = groupTagsByCategory([tag(1, 'Mer', 'Theme')]);

    expect(groups.find((group) => group.category === 'Occasion')?.tags).toEqual([]);
  });
});

describe('tagUsageCount', () => {
  it('additionne les chaînes et les breloques', () => {
    expect(tagUsageCount(tag(1, 'Mer', 'Theme', 1, 3))).toBe(4);
  });

  it('vaut 0 pour un tag inutilisé', () => {
    expect(tagUsageCount(tag(1, 'Mer', 'Theme'))).toBe(0);
  });
});

describe('deleteConfirmationMessage', () => {
  it('indique 4 articles pour un tag utilisé par 4 articles', () => {
    expect(deleteConfirmationMessage(tag(1, 'Mer', 'Theme', 1, 3))).toBe(
      'Ce tag est utilisé par 4 article(s). Le supprimer ?'
    );
  });

  it('indique 0 article pour un tag inutilisé', () => {
    expect(deleteConfirmationMessage(tag(1, 'Mer', 'Theme'))).toBe(
      'Ce tag est utilisé par 0 article(s). Le supprimer ?'
    );
  });
});

describe('mergeConfirmationMessage', () => {
  it('nomme le tag source et le tag conservé', () => {
    expect(mergeConfirmationMessage(tag(1, 'Océan', 'Theme'), tag(2, 'Mer', 'Theme'))).toBe(
      'Les articles de « Océan » passeront sur « Mer », puis « Océan » sera supprimé.'
    );
  });
});

describe('mergeTargetOptions', () => {
  it('propose tous les autres tags, toutes catégories, avec leur catégorie', () => {
    const ocean = tag(1, 'Océan', 'Theme');
    const tags = [ocean, tag(2, 'Mer', 'Theme'), tag(3, 'Doré', 'Color')];

    expect(mergeTargetOptions(ocean, tags)).toEqual([
      { value: '2', label: 'Mer (Thème)' },
      { value: '3', label: 'Doré (Couleur)' },
    ]);
  });

  it('ne propose rien quand le tag est le seul', () => {
    const ocean = tag(1, 'Océan', 'Theme');

    expect(mergeTargetOptions(ocean, [ocean])).toEqual([]);
  });
});

describe('validateTagName', () => {
  it.each(['Mer', '  Doré  ', '1', 'Cœur', 'Été'])('accepte le nom « %s »', (name) => {
    expect(validateTagName(name)).toBeUndefined();
  });

  it.each(['', '   '])('refuse un nom vide', (name) => {
    expect(validateTagName(name)).toBe(NAME_REQUIRED_MESSAGE);
  });

  it('accepte un nom de 50 caractères', () => {
    expect(validateTagName('a'.repeat(50))).toBeUndefined();
  });

  it('refuse un nom de 51 caractères', () => {
    expect(validateTagName('a'.repeat(51))).toBe(NAME_TOO_LONG_MESSAGE);
  });

  it('ne compte pas les espaces autour du nom dans la longueur', () => {
    expect(validateTagName(`  ${'a'.repeat(50)}  `)).toBeUndefined();
  });

  it.each(['---', '!!', '« »'])('refuse un nom sans lettre ni chiffre : %s', (name) => {
    expect(validateTagName(name)).toBe(NAME_WITHOUT_LETTER_OR_DIGIT_MESSAGE);
  });
});

describe('tagFormErrorsFromApi', () => {
  it('affiche sous le nom le message de l’API pour un nom déjà pris dans la catégorie (409)', () => {
    const err = apiError(409, {
      status: 409,
      title: 'Le tag « Mer » existe déjà dans cette catégorie.',
      errors: null,
    });

    expect(tagFormErrorsFromApi(err)).toEqual({
      fields: { name: 'Le tag « Mer » existe déjà dans cette catégorie.' },
      form: null,
    });
  });

  it('place chaque erreur de validation (400) sous son champ, quelle que soit la casse de la clé', () => {
    const err = apiError(400, {
      status: 400,
      title: 'Erreur de validation',
      errors: { Name: ['Le nom est requis.'], Category: ['Catégorie invalide.'] },
    });

    expect(tagFormErrorsFromApi(err)).toEqual({
      fields: { name: 'Le nom est requis.', category: 'Catégorie invalide.' },
      form: null,
    });
  });

  it('affiche en tête une erreur de validation sans champ connu', () => {
    const err = apiError(400, {
      status: 400,
      title: 'Erreur de validation',
      errors: { Id: ['Identifiant invalide.'] },
    });

    expect(tagFormErrorsFromApi(err)).toEqual({ fields: {}, form: 'Identifiant invalide.' });
  });

  it('affiche en tête un tag introuvable (404)', () => {
    const err = apiError(404, { status: 404, title: 'Tag introuvable.', errors: null });

    expect(tagFormErrorsFromApi(err)).toEqual({ fields: {}, form: 'Tag introuvable.' });
  });
});
