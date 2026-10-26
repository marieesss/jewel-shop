import { AxiosError, AxiosHeaders, type AxiosResponse } from 'axios';
import { describe, expect, it } from 'vitest';
import {
  LABEL_REQUIRED_MESSAGE,
  LABEL_TOO_LONG_MESSAGE,
  LENGTH_NOT_INTEGER_MESSAGE,
  LENGTH_OUT_OF_RANGE_MESSAGE,
  LENGTH_REQUIRED_MESSAGE,
  chainLengthFormErrorsFromApi,
  chainLengthFormFrom,
  deleteConfirmationMessage,
  toChainLengthInput,
  validateChainLengthForm,
} from './chainLengthForm';

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

describe('validateChainLengthForm', () => {
  it('accepte une longueur entière dans les bornes avec un libellé', () => {
    expect(validateChainLengthForm({ lengthCm: '45', label: 'Princesse' })).toEqual({});
  });

  it.each(['10', '200'])('accepte la borne %s cm', (lengthCm) => {
    expect(validateChainLengthForm({ lengthCm, label: 'Libellé' })).toEqual({});
  });

  it.each(['9', '201', '300', '0', '-45'])('refuse une longueur de %s cm', (lengthCm) => {
    expect(validateChainLengthForm({ lengthCm, label: 'Libellé' }).lengthCm).toBe(
      LENGTH_OUT_OF_RANGE_MESSAGE
    );
  });

  it('affiche le message de l’API pour une longueur de 300', () => {
    expect(validateChainLengthForm({ lengthCm: '300', label: 'Long' }).lengthCm).toBe(
      'La longueur doit être comprise entre 10 et 200 cm.'
    );
  });

  it.each(['', '   '])('refuse une longueur vide', (lengthCm) => {
    expect(validateChainLengthForm({ lengthCm, label: 'Libellé' }).lengthCm).toBe(
      LENGTH_REQUIRED_MESSAGE
    );
  });

  it.each(['45.5', '45,5', 'abc', '4e1'])('refuse une longueur non entière : %s', (lengthCm) => {
    expect(validateChainLengthForm({ lengthCm, label: 'Libellé' }).lengthCm).toBe(
      LENGTH_NOT_INTEGER_MESSAGE
    );
  });

  it.each(['', '   '])('refuse un libellé vide', (label) => {
    expect(validateChainLengthForm({ lengthCm: '45', label }).label).toBe(LABEL_REQUIRED_MESSAGE);
  });

  it('accepte un libellé de 50 caractères', () => {
    expect(validateChainLengthForm({ lengthCm: '45', label: 'a'.repeat(50) })).toEqual({});
  });

  it('refuse un libellé de 51 caractères', () => {
    expect(validateChainLengthForm({ lengthCm: '45', label: 'a'.repeat(51) }).label).toBe(
      LABEL_TOO_LONG_MESSAGE
    );
  });
});

describe('toChainLengthInput', () => {
  it('envoie la longueur en nombre et le libellé sans espaces autour', () => {
    expect(toChainLengthInput({ lengthCm: ' 45 ', label: '  Princesse ' })).toEqual({
      lengthCm: 45,
      label: 'Princesse',
    });
  });
});

describe('chainLengthFormFrom', () => {
  it('pré-remplit le formulaire avec la longueur à modifier', () => {
    expect(chainLengthFormFrom({ id: 3, lengthCm: 45, label: 'Princesse' })).toEqual({
      lengthCm: '45',
      label: 'Princesse',
    });
  });
});

describe('chainLengthFormErrorsFromApi', () => {
  it('affiche « Cette longueur existe déjà. » sous la longueur pour un 409', () => {
    const err = apiError(409, { status: 409, title: 'Cette longueur existe déjà.', errors: null });

    expect(chainLengthFormErrorsFromApi(err)).toEqual({
      fields: { lengthCm: 'Cette longueur existe déjà.' },
      form: null,
    });
  });

  it('place chaque erreur de validation sous son champ', () => {
    const err = apiError(400, {
      status: 400,
      title: 'Erreur de validation',
      errors: {
        LengthCm: ['La longueur doit être comprise entre 10 et 200 cm.'],
        Label: ['Le libellé est requis.'],
      },
    });

    expect(chainLengthFormErrorsFromApi(err)).toEqual({
      fields: {
        lengthCm: 'La longueur doit être comprise entre 10 et 200 cm.',
        label: 'Le libellé est requis.',
      },
      form: null,
    });
  });

  it('affiche en tête du formulaire une erreur de validation sans champ connu', () => {
    const err = apiError(400, {
      status: 400,
      title: 'Erreur de validation',
      errors: { Id: ['Identifiant invalide.'] },
    });

    expect(chainLengthFormErrorsFromApi(err)).toEqual({
      fields: {},
      form: 'Identifiant invalide.',
    });
  });

  it('affiche en tête du formulaire une autre erreur de l’API', () => {
    const err = apiError(404, { status: 404, title: 'Longueur de chaîne introuvable.' });

    expect(chainLengthFormErrorsFromApi(err)).toEqual({
      fields: {},
      form: 'Longueur de chaîne introuvable.',
    });
  });

  it('affiche en tête du formulaire une erreur réseau', () => {
    expect(chainLengthFormErrorsFromApi(new AxiosError('réseau'))).toEqual({
      fields: {},
      form: 'Impossible de joindre le serveur.',
    });
  });
});

describe('deleteConfirmationMessage', () => {
  it('demande la confirmation avec la longueur en cm', () => {
    expect(deleteConfirmationMessage({ id: 1, lengthCm: 45, label: 'Princesse' })).toBe(
      'Supprimer la longueur 45 cm ?'
    );
  });
});
