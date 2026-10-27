import { AxiosError } from 'axios';
import type { AddressInput, ProfileDto } from '../api/users';
import { extractApiError } from '../api/errors';

// Reprend UserAddressRules et UserMessages côté API.
export const ADDRESS_LINE_MAX_LENGTH = 255;
export const CITY_MAX_LENGTH = 100;
const POSTAL_CODE_PATTERN = /^\d{5}$/;
// Numéro français : 10 chiffres commençant par 0, ou +33 suivi des 9 chiffres sans le 0.
const PHONE_PATTERN = /^(0|\+33)[1-9]\d{8}$/;

export const ADDRESS_LINE1_REQUIRED_MESSAGE = "L'adresse est requise.";
export const ADDRESS_LINE_TOO_LONG_MESSAGE = '255 caractères maximum.';
export const POSTAL_CODE_INVALID_MESSAGE = 'Le code postal doit contenir 5 chiffres.';
export const CITY_REQUIRED_MESSAGE = 'La ville est requise.';
export const CITY_TOO_LONG_MESSAGE = '100 caractères maximum.';
export const PHONE_REQUIRED_MESSAGE = 'Le téléphone est requis.';
export const PHONE_INVALID_MESSAGE =
  'Le téléphone doit contenir 10 chiffres et commencer par 0, ou commencer par +33.';

export const ADDRESS_SAVED_MESSAGE = 'Adresse enregistrée.';

export const DELIVERY_COUNTRY = 'France';

const BAD_REQUEST = 400;

export interface AddressFormValues {
  line1: string;
  line2: string;
  postalCode: string;
  city: string;
  phone: string;
}

export type AddressField = keyof AddressFormValues;

export interface AddressFormErrors {
  fields: Partial<Record<AddressField, string>>;
  /** Erreur qui ne concerne aucun champ, affichée en tête du formulaire. */
  form: string | null;
}

export const noAddressFormErrors: AddressFormErrors = { fields: {}, form: null };

/** Pré-remplit le formulaire avec l'adresse et le téléphone déjà enregistrés. */
export function addressFormFrom(profile: ProfileDto): AddressFormValues {
  return {
    line1: profile.address?.line1 ?? '',
    line2: profile.address?.line2 ?? '',
    postalCode: profile.address?.postalCode ?? '',
    city: profile.address?.city ?? '',
    phone: profile.phone ?? '',
  };
}

function validateLine1(raw: string): string | undefined {
  const value = raw.trim();
  if (value === '') return ADDRESS_LINE1_REQUIRED_MESSAGE;
  if (value.length > ADDRESS_LINE_MAX_LENGTH) return ADDRESS_LINE_TOO_LONG_MESSAGE;
  return undefined;
}

function validateLine2(raw: string): string | undefined {
  if (raw.trim().length > ADDRESS_LINE_MAX_LENGTH) return ADDRESS_LINE_TOO_LONG_MESSAGE;
  return undefined;
}

function validatePostalCode(raw: string): string | undefined {
  if (!POSTAL_CODE_PATTERN.test(raw.trim())) return POSTAL_CODE_INVALID_MESSAGE;
  return undefined;
}

function validateCity(raw: string): string | undefined {
  const value = raw.trim();
  if (value === '') return CITY_REQUIRED_MESSAGE;
  if (value.length > CITY_MAX_LENGTH) return CITY_TOO_LONG_MESSAGE;
  return undefined;
}

function validatePhone(raw: string): string | undefined {
  const value = raw.trim();
  if (value === '') return PHONE_REQUIRED_MESSAGE;
  if (!PHONE_PATTERN.test(value)) return PHONE_INVALID_MESSAGE;
  return undefined;
}

const VALIDATORS: Record<AddressField, (raw: string) => string | undefined> = {
  line1: validateLine1,
  line2: validateLine2,
  postalCode: validatePostalCode,
  city: validateCity,
  phone: validatePhone,
};

/** Valide la saisie (miroir de UpdateMyAddressCommandValidator côté API). */
export function validateAddressForm(
  values: AddressFormValues
): Partial<Record<AddressField, string>> {
  const errors: Partial<Record<AddressField, string>> = {};

  for (const field of Object.keys(VALIDATORS) as AddressField[]) {
    const error = VALIDATORS[field](values[field]);
    if (error) errors[field] = error;
  }
  return errors;
}

/** Convertit une saisie déjà validée en corps de requête ; un complément vide est envoyé comme absent. */
export function toAddressInput(values: AddressFormValues): AddressInput {
  const line2 = values.line2.trim();
  return {
    line1: values.line1.trim(),
    line2: line2 === '' ? null : line2,
    postalCode: values.postalCode.trim(),
    city: values.city.trim(),
    phone: values.phone.trim(),
  };
}

// Les clés de `errors` sont les noms de propriétés C# (`PostalCode`) : la politique camelCase de l'API ne s'applique pas aux clés de dictionnaire.
const API_ERROR_KEYS: Record<string, AddressField> = {
  line1: 'line1',
  line2: 'line2',
  postalcode: 'postalCode',
  city: 'city',
  phone: 'phone',
};

function fromValidationError(errors: Record<string, string[]>): AddressFormErrors {
  const result: AddressFormErrors = { fields: {}, form: null };

  for (const [key, messages] of Object.entries(errors)) {
    const [message] = messages;
    if (!message) continue;

    const field = API_ERROR_KEYS[key.toLowerCase()];
    if (field) result.fields[field] ??= message;
    else result.form ??= message;
  }
  return result;
}

/** Répartit une erreur de l'API : un 400 sous le champ concerné, le reste en tête du formulaire. */
export function addressFormErrorsFromApi(err: unknown): AddressFormErrors {
  if (err instanceof AxiosError) {
    const status = err.response?.status;
    const data = err.response?.data as { errors?: Record<string, string[]> | null } | undefined;

    if (status === BAD_REQUEST && data?.errors) return fromValidationError(data.errors);
  }
  return { fields: {}, form: extractApiError(err) };
}
