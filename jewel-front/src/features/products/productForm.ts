import type { ProductColor } from '../../api/products';

/** Champs communs aux deux types de produit (breloque & chaîne). */
export interface ProductCommonValues {
  name: string;
  description: string;
  color: ProductColor;
  url: string;
  cost: string;
  price: string;
}

export const emptyProductCommon: ProductCommonValues = {
  name: '',
  description: '',
  color: 'Gold',
  url: '',
  cost: '',
  price: '',
};

export type FieldErrors<T extends string> = Partial<Record<T, string>>;

/** Pré-remplit les champs communs à partir d'un produit existant (édition). */
export function commonFromProduct(p: {
  name: string;
  description: string;
  color: string;
  url: string | null;
  cost: number;
  price: number;
}): ProductCommonValues {
  return {
    name: p.name,
    description: p.description,
    color: p.color as ProductColor,
    url: p.url ?? '',
    cost: String(p.cost),
    price: String(p.price),
  };
}

/** Convertit une saisie (avec virgule éventuelle) en nombre, ou NaN si vide/invalide. */
export function toNumber(raw: string): number {
  const normalized = raw.trim().replace(',', '.');
  if (normalized === '') return NaN;
  return Number(normalized);
}

/** Valide les champs communs (miroir des FluentValidation côté back). */
export function validateCommon(
  values: ProductCommonValues
): FieldErrors<keyof ProductCommonValues> {
  const errors: FieldErrors<keyof ProductCommonValues> = {};

  if (!values.name.trim()) errors.name = 'Le nom est requis.';
  else if (values.name.trim().length > 255) errors.name = '255 caractères maximum.';

  if (values.description.length > 10000) errors.description = '10000 caractères maximum.';

  if (values.url.trim().length > 500) errors.url = '500 caractères maximum.';

  const cost = toNumber(values.cost);
  if (Number.isNaN(cost)) errors.cost = 'Coût requis.';
  else if (cost < 0) errors.cost = 'Le coût doit être positif.';

  const price = toNumber(values.price);
  if (Number.isNaN(price)) errors.price = 'Prix requis.';
  else if (price < 0) errors.price = 'Le prix doit être positif.';

  return errors;
}
