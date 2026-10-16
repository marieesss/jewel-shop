import { describe, expect, it } from 'vitest';
import { loginMessageOf } from './loginMessage';

describe('loginMessageOf', () => {
  it('renvoie le message transmis à la page de connexion', () => {
    const message = 'Votre mot de passe a été modifié. Vous pouvez vous reconnecter.';
    expect(loginMessageOf({ message })).toBe(message);
  });

  it.each([
    ['un état absent', null],
    ['un état sans message', { sessionExpired: true }],
    ['un message vide', { message: '' }],
    ['un message qui n’est pas un texte', { message: 42 }],
    ['un état qui n’est pas un objet', 'message'],
  ])('ignore %s', (_label, state) => {
    expect(loginMessageOf(state)).toBeNull();
  });
});
