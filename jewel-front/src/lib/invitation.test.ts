import { AxiosError, AxiosHeaders, type AxiosResponse } from 'axios';
import { describe, expect, it } from 'vitest';
import { isInvalidInvitationError, isInvitationNotFoundError } from './invitation';

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
    errors: { [key]: ['Cette invitation est invalide ou a expiré.'] },
  });
}

describe('isInvitationNotFoundError', () => {
  it('reconnaît un 404', () => {
    expect(isInvitationNotFoundError(apiError(404, { status: 404 }))).toBe(true);
  });

  it.each([400, 500])('ignore une erreur %i', (status) => {
    expect(isInvitationNotFoundError(apiError(status, {}))).toBe(false);
  });

  it('ignore une erreur qui ne vient pas de l’API', () => {
    expect(isInvitationNotFoundError(new Error('réseau'))).toBe(false);
  });
});

describe('isInvalidInvitationError', () => {
  it.each(['InvitationToken', 'invitationToken'])(
    'reconnaît une erreur de validation sur la clé %s',
    (key) => {
      expect(isInvalidInvitationError(validationError(key))).toBe(true);
    }
  );

  it('ignore une erreur de validation sur le mot de passe', () => {
    expect(isInvalidInvitationError(validationError('Password'))).toBe(false);
  });

  it('ignore un 400 sans erreurs détaillées', () => {
    expect(isInvalidInvitationError(apiError(400, { status: 400, errors: null }))).toBe(false);
  });

  it('ignore une erreur serveur', () => {
    expect(isInvalidInvitationError(apiError(500, { errors: { InvitationToken: ['x'] } }))).toBe(
      false
    );
  });

  it('ignore une erreur qui ne vient pas de l’API', () => {
    expect(isInvalidInvitationError(new Error('réseau'))).toBe(false);
  });
});
