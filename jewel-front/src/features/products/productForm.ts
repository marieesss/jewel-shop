import type { ProductColor } from '../../api/products';
import type { TagDto } from '../../api/tags';

/** Champs communs aux deux types de produit (breloque & chaîne). */
export interface ProductCommonValues {
  name: string;
  description: string;
  color: ProductColor;
  url: string;
  cost: string;
  price: string;
  tagIds: number[];
}

export const emptyProductCommon: ProductCommonValues = {
  name: '',
  description: '',
  color: 'Gold',
  url: '',
  cost: '',
  price: '',
  tagIds: [],
};

export type FieldErrors<T extends string> = Partial<Record<T, string>>;

/** Pré-remplit les champs communs à partir d'un produit existant (édition). */
export function commonFromProduct(p: {
  name: string;
  description: string;
  color: string;
  url?: string | null;
  cost: number;
  price: number;
  tags?: TagDto[] | null;
}): ProductCommonValues {
  return {
    name: p.name,
    description: p.description,
    color: p.color as ProductColor,
    url: p.url ?? '',
    cost: String(p.cost),
    price: String(p.price),
    tagIds: tagIdsOf(p.tags),
  };
}

/** Ids des tags d'un produit, pour pré-remplir la sélection (aucun tag si la réponse n'en a pas). */
export function tagIdsOf(tags: TagDto[] | null | undefined): number[] {
  return tags?.map((tag) => tag.id) ?? [];
}

/** Convertit une saisie (avec virgule éventuelle) en nombre, ou NaN si vide/invalide. */
export function toNumber(raw: string): number {
  const normalized = raw.trim().replace(',', '.');
  if (normalized === '') return NaN;
  return Number(normalized);
}

export interface RequiredPriceMessages {
  cost: string;
  price: string;
}

export const COST_MESSAGE = 'Le coût doit être positif.';
export const PRICE_MESSAGE = 'Le prix doit être positif.';

/** Valide un montant obligatoire et positif ; renvoie le message d'erreur, ou `undefined`. */
export function validateAmount(raw: string, requiredMessage: string, negativeMessage: string) {
  const amount = toNumber(raw);
  if (Number.isNaN(amount)) return requiredMessage;
  if (amount < 0) return negativeMessage;
  return undefined;
}

const DEFAULT_REQUIRED_PRICE_MESSAGES: RequiredPriceMessages = {
  cost: 'Coût requis.',
  price: 'Prix requis.',
};

/** Valide les champs communs (miroir des FluentValidation côté back). */
export function validateCommon(
  values: ProductCommonValues,
  requiredPriceMessages: RequiredPriceMessages = DEFAULT_REQUIRED_PRICE_MESSAGES
): FieldErrors<keyof ProductCommonValues> {
  const errors: FieldErrors<keyof ProductCommonValues> = {};

  if (!values.name.trim()) errors.name = 'Le nom est requis.';
  else if (values.name.trim().length > 255) errors.name = '255 caractères maximum.';

  if (values.description.length > 10000) errors.description = '10000 caractères maximum.';

  if (values.url.trim().length > 500) errors.url = '500 caractères maximum.';

  const costError = validateAmount(values.cost, requiredPriceMessages.cost, COST_MESSAGE);
  if (costError) errors.cost = costError;

  const priceError = validateAmount(values.price, requiredPriceMessages.price, PRICE_MESSAGE);
  if (priceError) errors.price = priceError;

  return errors;
}
