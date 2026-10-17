import { describe, expect, it } from 'vitest';
import { INVITATION_EMAIL_MAX_LENGTH, validateInvitationEmail } from './invitationEmail';

const emailOfLength = (length: number) =>
  `${'a'.repeat(length - '@exemple.com'.length)}@exemple.com`;

describe('validateInvitationEmail', () => {
  it('accepte un email valide', () => {
    expect(validateInvitationEmail('marie@exemple.com')).toEqual([]);
  });

  it('exige un email', () => {
    expect(validateInvitationEmail('')[0]).toBe("L'email est requis.");
  });

  it('refuse un email mal formé', () => {
    expect(validateInvitationEmail('marie.exemple.com')).toEqual(["L'email n'est pas valide."]);
  });

  it('accepte un email de exactement 255 caractères', () => {
    expect(validateInvitationEmail(emailOfLength(INVITATION_EMAIL_MAX_LENGTH))).toEqual([]);
  });

  it('refuse un email de plus de 255 caractères', () => {
    expect(validateInvitationEmail(emailOfLength(INVITATION_EMAIL_MAX_LENGTH + 1))).toEqual([
      '255 caractères maximum.',
    ]);
  });
});
