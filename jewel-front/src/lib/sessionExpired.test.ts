import { describe, expect, it } from 'vitest';
import { isSessionExpiredState } from './sessionExpired';

describe('isSessionExpiredState', () => {
  it("reconnaît l'état transmis après l'expiration de la session", () => {
    expect(isSessionExpiredState({ sessionExpired: true })).toBe(true);
  });

  it.each([
    ['un état absent', null],
    ['un état sans expiration', {}],
    ['une expiration à faux', { sessionExpired: false }],
    ['un état qui n’est pas un objet', 'sessionExpired'],
  ])('ignore %s', (_label, state) => {
    expect(isSessionExpiredState(state)).toBe(false);
  });
});
