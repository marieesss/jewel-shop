/** Nom du paramètre d'adresse qui porte les tags cochés (`/charms?tags=mer,dore`). */
export const TAGS_PARAM = 'tags';

const TAG_SLUG_SEPARATOR = ',';

/** Slugs lus dans le paramètre `tags`, sans espace, valeur vide ni doublon, dans l'ordre de l'adresse. */
export function parseTagSlugs(param: string | null): string[] {
  if (!param) return [];
  const slugs = param
    .split(TAG_SLUG_SEPARATOR)
    .map((slug) => slug.trim())
    .filter((slug) => slug !== '');
  return [...new Set(slugs)];
}

/** Valeur du paramètre `tags` envoyée à l'API, absente quand aucun tag n'est coché. */
export function serializeTagSlugs(slugs: string[]): string | undefined {
  return slugs.length > 0 ? slugs.join(TAG_SLUG_SEPARATOR) : undefined;
}

/** Copie des paramètres d'adresse avec `tags` à jour, retiré quand aucun tag n'est coché. */
export function withTagSlugs(params: URLSearchParams, slugs: string[]): URLSearchParams {
  const next = new URLSearchParams(params);
  const value = serializeTagSlugs(slugs);
  if (value === undefined) next.delete(TAGS_PARAM);
  else next.set(TAGS_PARAM, value);
  return next;
}

/** Coche un slug (ajouté en fin de liste) ou le décoche. */
export function toggleTagSlug(slugs: string[], slug: string): string[] {
  return slugs.includes(slug) ? slugs.filter((s) => s !== slug) : [...slugs, slug];
}

/** Page choisie, mémorisée avec les tags cochés au moment du choix. */
export interface TagFilteredPage {
  page: number;
  tagsParam: string | null;
}

/** Page à afficher : la page mémorisée si les tags n'ont pas changé depuis, sinon la première. */
export function currentPage(state: TagFilteredPage, tagsParam: string | null): number {
  return state.tagsParam === tagsParam ? state.page : 1;
}
