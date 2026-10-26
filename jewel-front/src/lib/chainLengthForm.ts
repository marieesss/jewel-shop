import { AxiosError } from 'axios';
import type { ChainLength, ChainLengthInput } from '../api/chainLengths';
import { extractApiError } from '../api/errors';

// Reprend ChainLengthRules et ChainLengthMessages côté API.
export const MIN_LENGTH_CM = 10;
export const MAX_LENGTH_CM = 200;
export const LABEL_MAX_LENGTH = 50;

export const LENGTH_REQUIRED_MESSAGE = 'La longueur est requise.';
export const LENGTH_NOT_INTEGER_MESSAGE = 'La longueur doit être un nombre entier.';
export const LENGTH_OUT_OF_RANGE_MESSAGE = 'La longueur doit être comprise entre 10 et 200 cm.';
export const LABEL_REQUIRED_MESSAGE = 'Le libellé est requis.';
export const LABEL_TOO_LONG_MESSAGE = '50 caractères maximum.';

export const CREATE_SUCCESS_MESSAGE = 'Longueur ajoutée.';
export const UPDATE_SUCCESS_MESSAGE = 'Longueur modifiée.';
export const DELETE_SUCCESS_MESSAGE = 'Longueur supprimée.';

const BAD_REQUEST = 400;
const CONFLICT = 409;

const INTEGER_PATTERN = /^-?\d+$/;

export interface ChainLengthFormValues {
  lengthCm: string;
  label: string;
}

export type ChainLengthField = keyof ChainLengthFormValues;

export interface ChainLengthFormErrors {
  fields: Partial<Record<ChainLengthField, string>>;
  /** Erreur qui ne concerne aucun champ, affichée en tête du formulaire. */
  form: string | null;
}

export const emptyChainLengthForm: ChainLengthFormValues = { lengthCm: '', label: '' };

export const noChainLengthFormErrors: ChainLengthFormErrors = { fields: {}, form: null };

export function chainLengthFormFrom(length: ChainLength): ChainLengthFormValues {
  return { lengthCm: String(length.lengthCm), label: length.label };
}

function validateLengthCm(raw: string): string | undefined {
  const value = raw.trim();
  if (value === '') return LENGTH_REQUIRED_MESSAGE;
  if (!INTEGER_PATTERN.test(value)) return LENGTH_NOT_INTEGER_MESSAGE;

  const lengthCm = Number(value);
  if (lengthCm < MIN_LENGTH_CM || lengthCm > MAX_LENGTH_CM) return LENGTH_OUT_OF_RANGE_MESSAGE;
  return undefined;
}

function validateLabel(raw: string): string | undefined {
  const value = raw.trim();
  if (value === '') return LABEL_REQUIRED_MESSAGE;
  if (value.length > LABEL_MAX_LENGTH) return LABEL_TOO_LONG_MESSAGE;
  return undefined;
}

/** Valide la saisie (miroir de ChainLengthFieldsValidator côté API). */
export function validateChainLengthForm(
  values: ChainLengthFormValues
): Partial<Record<ChainLengthField, string>> {
  const errors: Partial<Record<ChainLengthField, string>> = {};

  const lengthError = validateLengthCm(values.lengthCm);
  if (lengthError) errors.lengthCm = lengthError;

  const labelError = validateLabel(values.label);
  if (labelError) errors.label = labelError;

  return errors;
}

/** Convertit une saisie déjà validée en corps de requête. */
export function toChainLengthInput(values: ChainLengthFormValues): ChainLengthInput {
  return { lengthCm: Number(values.lengthCm.trim()), label: values.label.trim() };
}

// Les clés de `errors` sont les noms de propriétés C# (`LengthCm`) : la politique camelCase de l'API ne s'applique pas aux clés de dictionnaire.
const API_ERROR_KEYS: Record<string, ChainLengthField> = {
  lengthcm: 'lengthCm',
  label: 'label',
};

function fromValidationError(errors: Record<string, string[]>): ChainLengthFormErrors {
  const result: ChainLengthFormErrors = { fields: {}, form: null };

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
 * Répartit une erreur de l'API entre les champs du formulaire : un 400 sous le champ
 * concerné, un 409 (longueur déjà existante) sous la longueur, le reste en tête du formulaire.
 */
export function chainLengthFormErrorsFromApi(err: unknown): ChainLengthFormErrors {
  if (err instanceof AxiosError) {
    const status = err.response?.status;
    const data = err.response?.data as { errors?: Record<string, string[]> | null } | undefined;

    if (status === BAD_REQUEST && data?.errors) return fromValidationError(data.errors);
    if (status === CONFLICT) return { fields: { lengthCm: extractApiError(err) }, form: null };
  }
  return { fields: {}, form: extractApiError(err) };
}

export function deleteConfirmationMessage(length: ChainLength): string {
  return `Supprimer la longueur ${length.lengthCm} cm ?`;
}
