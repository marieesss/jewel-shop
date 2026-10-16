import { AxiosError, AxiosHeaders, type AxiosResponse } from 'axios';
import { describe, expect, it } from 'vitest';
import { INVALID_RESET_TOKEN_MESSAGE, isInvalidResetTokenError } from './passwordReset';

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

function validationError(key: string): AxiosError {
  return apiError(400, {
    status: 400,
    title: 'Erreur de validation',
    errors: { [key]: [INVALID_RESET_TOKEN_MESSAGE] },
  });
}

describe('isInvalidResetTokenError', () => {
  it.each(['Token', 'token'])('reconnaît une erreur de validation sur la clé %s', (key) => {
    expect(isInvalidResetTokenError(validationError(key))).toBe(true);
  });

  it('ignore une erreur de validation sur le mot de passe', () => {
    expect(isInvalidResetTokenError(validationError('Password'))).toBe(false);
  });

  it('ignore un 400 sans erreurs détaillées', () => {
    expect(isInvalidResetTokenError(apiError(400, { status: 400, errors: null }))).toBe(false);
  });

  it('ignore une erreur serveur', () => {
    expect(isInvalidResetTokenError(apiError(500, { errors: { Token: ['x'] } }))).toBe(false);
  });

  it('ignore une erreur réseau', () => {
    expect(isInvalidResetTokenError(new AxiosError('Network Error'))).toBe(false);
  });

  it('ignore une erreur qui ne vient pas d’Axios', () => {
    expect(isInvalidResetTokenError(new Error('boom'))).toBe(false);
  });
});
