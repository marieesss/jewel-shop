import { AxiosError } from 'axios';
import { extractApiError } from '../api/errors';
import { TAG_CATEGORIES, type AdminTag, type Tag, type TagCategory } from '../api/tags';
import type { SelectOption } from '../components/ui/Select';

// Reprend TagRules et TagMessages côté API.
export const TAG_NAME_MAX_LENGTH = 50;

export const NAME_REQUIRED_MESSAGE = 'Le nom est requis.';
export const NAME_TOO_LONG_MESSAGE = '50 caractères maximum.';
export const NAME_WITHOUT_LETTER_OR_DIGIT_MESSAGE =
  'Le nom doit contenir au moins une lettre ou un chiffre.';

export const CREATE_SUCCESS_MESSAGE = 'Tag ajouté.';
export const RENAME_SUCCESS_MESSAGE = 'Tag renommé.';
export const DELETE_SUCCESS_MESSAGE = 'Tag supprimé.';
export const MERGE_SUCCESS_MESSAGE = 'Tags fusionnés.';

export const DEFAULT_TAG_CATEGORY: TagCategory = 'Theme';

export const TAG_CATEGORY_LABELS: Record<TagCategory, string> = {
  Theme: 'Thème',
  Color: 'Couleur',
  Material: 'Matière',
  Occasion: 'Occasion',
};

export const TAG_CATEGORY_OPTIONS: (SelectOption & { value: TagCategory })[] = TAG_CATEGORIES.map(
  (category) => ({ value: category, label: TAG_CATEGORY_LABELS[category] })
);

const BAD_REQUEST = 400;
const CONFLICT = 409;

const DIACRITICS = /\p{Mn}/gu;
const LETTER_OR_DIGIT = /[a-z0-9]/;

export type TagField = 'name' | 'category';

export interface TagFormErrors {
  fields: Partial<Record<TagField, string>>;
  /** Erreur qui ne concerne aucun champ, affichée en tête du formulaire. */
  form: string | null;
}

export const noTagFormErrors: TagFormErrors = { fields: {}, form: null };

export interface TagCategoryGroup {
  category: TagCategory;
  label: string;
  tags: AdminTag[];
}

/** Nombre d'articles (chaînes et breloques) qui portent le tag. */
export function tagUsageCount(tag: AdminTag): number {
  return tag.chainCount + tag.charmCount;
}

/** Regroupe les tags sous chaque catégorie, dans l'ordre de l'API, en gardant l'ordre de la liste. */
export function groupTagsByCategory(tags: AdminTag[]): TagCategoryGroup[] {
  return TAG_CATEGORIES.map((category) => ({
    category,
    label: TAG_CATEGORY_LABELS[category],
    tags: tags.filter((tag) => tag.category === category),
  }));
}

// Miroir de TagSlug.CanBeBuiltFrom côté API : le slug calculé depuis le nom ne doit pas être vide.
function canBuildSlugFrom(name: string): boolean {
  const lowered = name.toLowerCase().replace(/œ/g, 'oe').replace(/æ/g, 'ae');
  return LETTER_OR_DIGIT.test(lowered.normalize('NFD').replace(DIACRITICS, ''));
}

/** Valide le nom d'un tag (miroir de TagNameRules côté API). */
export function validateTagName(raw: string): string | undefined {
  const name = raw.trim();
  if (name === '') return NAME_REQUIRED_MESSAGE;
  if (name.length > TAG_NAME_MAX_LENGTH) return NAME_TOO_LONG_MESSAGE;
  if (!canBuildSlugFrom(name)) return NAME_WITHOUT_LETTER_OR_DIGIT_MESSAGE;
  return undefined;
}

// Les clés de `errors` sont les noms de propriétés C# (`Name`) : la politique camelCase de l'API ne s'applique pas aux clés de dictionnaire.
const API_ERROR_KEYS: Record<string, TagField> = {
  name: 'name',
  category: 'category',
};

function fromValidationError(errors: Record<string, string[]>): TagFormErrors {
  const result: TagFormErrors = { fields: {}, form: null };

  for (const [key, messages] of Object.entries(errors)) {
    const [message] = messages;
    if (!message) continue;

    const field = API_ERROR_KEYS[key.toLowerCase()];
    if (field) result.fields[field] ??= message;
    else result.form ??= message;
  }
  return result;
}

/**
 * Répartit une erreur de l'API : un 400 sous le champ concerné, un 409 (nom déjà pris
 * ou trop proche dans la catégorie) sous le nom, le reste (404…) en tête du formulaire.
 */
export function tagFormErrorsFromApi(err: unknown): TagFormErrors {
  if (err instanceof AxiosError) {
    const status = err.response?.status;
    const data = err.response?.data as { errors?: Record<string, string[]> | null } | undefined;

    if (status === BAD_REQUEST && data?.errors) return fromValidationError(data.errors);
    if (status === CONFLICT) return { fields: { name: extractApiError(err) }, form: null };
  }
  return { fields: {}, form: extractApiError(err) };
}

/** Tags vers lesquels le tag peut être fusionné : tous les autres, toutes catégories confondues. */
export function mergeTargetOptions(source: Tag, tags: Tag[]): SelectOption[] {
  return tags
    .filter((tag) => tag.id !== source.id)
    .map((tag) => ({
      value: String(tag.id),
      label: `${tag.name} (${TAG_CATEGORY_LABELS[tag.category]})`,
    }));
}

export function deleteConfirmationMessage(tag: AdminTag): string {
  return `Ce tag est utilisé par ${tagUsageCount(tag)} article(s). Le supprimer ?`;
}

export function mergeConfirmationMessage(source: Tag, target: Tag): string {
  return `Les articles de « ${source.name} » passeront sur « ${target.name} », puis « ${source.name} » sera supprimé.`;
}
