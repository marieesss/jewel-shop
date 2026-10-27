import { AxiosError, AxiosHeaders, type AxiosResponse } from 'axios';
import { describe, expect, it } from 'vitest';
import type { ProfileDto } from '../api/users';
import {
  ADDRESS_LINE1_REQUIRED_MESSAGE,
  ADDRESS_LINE_TOO_LONG_MESSAGE,
  CITY_REQUIRED_MESSAGE,
  CITY_TOO_LONG_MESSAGE,
  PHONE_INVALID_MESSAGE,
  PHONE_REQUIRED_MESSAGE,
  POSTAL_CODE_INVALID_MESSAGE,
  addressFormErrorsFromApi,
  addressFormFrom,
  toAddressInput,
  validateAddressForm,
  type AddressFormValues,
} from './addressForm';

const validForm: AddressFormValues = {
  line1: '12 rue des Lilas',
  line2: '',
  postalCode: '75011',
  city: 'Paris',
  phone: '0612345678',
};

const profile: ProfileDto = {
  id: 7,
  name: 'Durand',
  surname: 'Alice',
  email: 'alice@example.com',
  birthday: '1990-04-12',
  phone: null,
  address: null,
};

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

describe('validateAddressForm', () => {
  it('accepte une adresse complète sans complément', () => {
    expect(validateAddressForm(validForm)).toEqual({});
  });

  it('accepte une adresse avec complément', () => {
    expect(validateAddressForm({ ...validForm, line2: 'Bâtiment B' })).toEqual({});
  });

  it.each(['', '   '])('refuse une adresse vide', (line1) => {
    expect(validateAddressForm({ ...validForm, line1 }).line1).toBe(ADDRESS_LINE1_REQUIRED_MESSAGE);
  });

  it('accepte une adresse de 255 caractères', () => {
    expect(validateAddressForm({ ...validForm, line1: 'a'.repeat(255) })).toEqual({});
  });

  it('refuse une adresse de 256 caractères', () => {
    expect(validateAddressForm({ ...validForm, line1: 'a'.repeat(256) }).line1).toBe(
      ADDRESS_LINE_TOO_LONG_MESSAGE
    );
  });

  it('refuse un complément de 256 caractères', () => {
    expect(validateAddressForm({ ...validForm, line2: 'a'.repeat(256) }).line2).toBe(
      ADDRESS_LINE_TOO_LONG_MESSAGE
    );
  });

  it('affiche « Le code postal doit contenir 5 chiffres. » pour un code postal de 4 chiffres', () => {
    expect(validateAddressForm({ ...validForm, postalCode: '7501' }).postalCode).toBe(
      'Le code postal doit contenir 5 chiffres.'
    );
  });

  it.each(['', '750111', '75 011', '7501A', '2A004'])(
    'refuse le code postal « %s »',
    (postalCode) => {
      expect(validateAddressForm({ ...validForm, postalCode }).postalCode).toBe(
        POSTAL_CODE_INVALID_MESSAGE
      );
    }
  );

  it.each(['', '   '])('refuse une ville vide', (city) => {
    expect(validateAddressForm({ ...validForm, city }).city).toBe(CITY_REQUIRED_MESSAGE);
  });

  it('accepte une ville de 100 caractères', () => {
    expect(validateAddressForm({ ...validForm, city: 'a'.repeat(100) })).toEqual({});
  });

  it('refuse une ville de 101 caractères', () => {
    expect(validateAddressForm({ ...validForm, city: 'a'.repeat(101) }).city).toBe(
      CITY_TOO_LONG_MESSAGE
    );
  });

  it.each(['0612345678', '0123456789', '+33612345678'])('accepte le téléphone %s', (phone) => {
    expect(validateAddressForm({ ...validForm, phone })).toEqual({});
  });

  it.each(['', '   '])('refuse un téléphone vide', (phone) => {
    expect(validateAddressForm({ ...validForm, phone }).phone).toBe(PHONE_REQUIRED_MESSAGE);
  });

  it.each([
    '061234567',
    '06123456789',
    '0012345678',
    '6123456789',
    '+330612345678',
    '+3361234567',
    '06 12 34 56 78',
    '06.12.34.56.78',
  ])('refuse le téléphone « %s »', (phone) => {
    expect(validateAddressForm({ ...validForm, phone }).phone).toBe(PHONE_INVALID_MESSAGE);
  });
});

describe('toAddressInput', () => {
  it('envoie les valeurs sans espaces autour', () => {
    expect(
      toAddressInput({
        line1: ' 12 rue des Lilas ',
        line2: ' Bâtiment B ',
        postalCode: ' 75011 ',
        city: ' Paris ',
        phone: ' 0612345678 ',
      })
    ).toEqual({
      line1: '12 rue des Lilas',
      line2: 'Bâtiment B',
      postalCode: '75011',
      city: 'Paris',
      phone: '0612345678',
    });
  });

  it.each(['', '   '])('envoie un complément vide comme absent', (line2) => {
    expect(toAddressInput({ ...validForm, line2 }).line2).toBeNull();
  });
});

describe('addressFormFrom', () => {
  it('laisse le formulaire vide tant qu’aucune adresse n’est enregistrée', () => {
    expect(addressFormFrom(profile)).toEqual({
      line1: '',
      line2: '',
      postalCode: '',
      city: '',
      phone: '',
    });
  });

  it('pré-remplit le formulaire avec l’adresse et le téléphone enregistrés', () => {
    expect(
      addressFormFrom({
        ...profile,
        phone: '+33612345678',
        address: { line1: '12 rue des Lilas', line2: null, postalCode: '75011', city: 'Paris' },
      })
    ).toEqual({
      line1: '12 rue des Lilas',
      line2: '',
      postalCode: '75011',
      city: 'Paris',
      phone: '+33612345678',
    });
  });
});

describe('addressFormErrorsFromApi', () => {
  it('place chaque erreur de validation sous son champ', () => {
    const err = apiError(400, {
      status: 400,
      title: 'Erreur de validation',
      errors: {
        Line1: ["L'adresse est requise."],
        PostalCode: ['Le code postal doit contenir 5 chiffres.'],
        Phone: ['Le téléphone est requis.'],
      },
    });

    expect(addressFormErrorsFromApi(err)).toEqual({
      fields: {
        line1: "L'adresse est requise.",
        postalCode: 'Le code postal doit contenir 5 chiffres.',
        phone: 'Le téléphone est requis.',
      },
      form: null,
    });
  });

  it('affiche en tête du formulaire une erreur de validation sans champ connu', () => {
    const err = apiError(400, {
      status: 400,
      title: 'Erreur de validation',
      errors: { UserId: ['Identifiant invalide.'] },
    });

    expect(addressFormErrorsFromApi(err)).toEqual({ fields: {}, form: 'Identifiant invalide.' });
  });

  it('affiche en tête du formulaire une autre erreur de l’API', () => {
    const err = apiError(404, { status: 404, title: 'Utilisateur introuvable.' });

    expect(addressFormErrorsFromApi(err)).toEqual({ fields: {}, form: 'Utilisateur introuvable.' });
  });

  it('affiche en tête du formulaire une erreur réseau', () => {
    expect(addressFormErrorsFromApi(new AxiosError('réseau'))).toEqual({
      fields: {},
      form: 'Impossible de joindre le serveur.',
    });
  });
});
