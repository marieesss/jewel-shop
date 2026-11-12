import { TAG_CATEGORIES, TAG_CATEGORY_LABELS, type TagCategory, type TagDto } from '../../api/tags';

export interface TagGroup {
  category: TagCategory;
  label: string;
  tags: TagDto[];
}

/** Miroir de `TagRules` côté back. */
export const TAG_NAME_MAX_LENGTH = 50;
const TAG_SLUG_MAX_LENGTH = 60;

export const TAG_NAME_REQUIRED_MESSAGE = 'Le nom est requis.';
export const TAG_NAME_TOO_LONG_MESSAGE = '50 caractères maximum.';
export const TAG_NAME_WITHOUT_LETTER_OR_DIGIT_MESSAGE =
  'Le nom doit contenir au moins une lettre ou un chiffre.';

const DIACRITICS = /[̀-ͯ]/g;

/** Minuscules sans accents, `œ` et `æ` développés : base commune de la recherche et du slug. */
function simplify(text: string): string {
  return text
    .trim()
    .toLowerCase()
    .replace(/œ/g, 'oe')
    .replace(/æ/g, 'ae')
    .normalize('NFD')
    .replace(DIACRITICS, '');
}

/** Slug d'un nom de tag, calculé comme `TagSlug.From` côté back. */
export function tagSlug(name: string): string {
  const slug = simplify(name)
    .replace(/[^a-z0-9]+/g, '-')
    .replace(/^-+|-+$/g, '');
  return slug.length <= TAG_SLUG_MAX_LENGTH
    ? slug
    : slug.slice(0, TAG_SLUG_MAX_LENGTH).replace(/-+$/, '');
}

/** Valide le nom d'un nouveau tag (miroir de `TagNameRules` côté back). */
export function validateTagName(name: string): string | undefined {
  if (!name.trim()) return TAG_NAME_REQUIRED_MESSAGE;
  if (name.trim().length > TAG_NAME_MAX_LENGTH) return TAG_NAME_TOO_LONG_MESSAGE;
  if (!tagSlug(name)) return TAG_NAME_WITHOUT_LETTER_OR_DIGIT_MESSAGE;
  return undefined;
}

function byName(a: TagDto, b: TagDto): number {
  return a.name.localeCompare(b.name, 'fr', { sensitivity: 'base' });
}

/**
 * Tags qui contiennent la recherche (sans tenir compte de la casse ni des accents), groupés par
 * catégorie dans l'ordre de l'API puis triés par nom. Les catégories sans tag sont omises.
 */
export function groupTagsByCategory(tags: TagDto[], search: string): TagGroup[] {
  const simplifiedSearch = simplify(search);
  const matching = tags.filter((tag) => simplify(tag.name).includes(simplifiedSearch));

  return TAG_CATEGORIES.map((category) => ({
    category,
    label: TAG_CATEGORY_LABELS[category],
    tags: matching.filter((tag) => tag.category === category).sort(byName),
  })).filter((group) => group.tags.length > 0);
}

/** Tags sélectionnés, dans l'ordre de la liste (catégorie puis nom). */
export function selectedTags(tags: TagDto[], selectedIds: number[]): TagDto[] {
  return groupTagsByCategory(tags, '')
    .flatMap((group) => group.tags)
    .filter((tag) => selectedIds.includes(tag.id));
}

/** Ajoute l'id s'il n'est pas sélectionné, le retire sinon. */
export function toggleTagId(selectedIds: number[], id: number): number[] {
  return selectedIds.includes(id)
    ? selectedIds.filter((selectedId) => selectedId !== id)
    : [...selectedIds, id];
}

/** Ajoute un tag créé à la liste, sans doublon. */
export function withTag(tags: TagDto[], tag: TagDto): TagDto[] {
  return tags.some((existing) => existing.id === tag.id) ? tags : [...tags, tag];
}

function sameName(a: string, b: string): boolean {
  return a.trim().toLowerCase() === b.trim().toLowerCase();
}

/**
 * Catégories où un tag de ce nom peut être créé : celles où aucun tag ne porte déjà ce nom,
 * sans tenir compte de la casse (même règle d'unicité que l'API). Vide si la saisie est vide.
 */
export function categoriesAvailableFor(name: string, tags: TagDto[]): TagCategory[] {
  if (!name.trim()) return [];
  return TAG_CATEGORIES.filter(
    (category) => !tags.some((tag) => tag.category === category && sameName(tag.name, name))
  );
}
