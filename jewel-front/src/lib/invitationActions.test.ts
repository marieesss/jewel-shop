import { describe, expect, it } from 'vitest';
import {
  DEFAULT_INVITATION_STATUS,
  INVITATION_STATUS_OPTIONS,
  canResendInvitation,
  canRevokeInvitation,
  resendSuccessMessage,
  revokeConfirmationMessage,
} from './invitationActions';

describe('INVITATION_STATUS_OPTIONS', () => {
  it('propose les quatre statuts dans l’ordre de la spécification', () => {
    expect(INVITATION_STATUS_OPTIONS).toEqual([
      { value: 'Pending', label: 'En attente' },
      { value: 'Expired', label: 'Expirées' },
      { value: 'Revoked', label: 'Révoquées' },
      { value: 'Accepted', label: 'Acceptées' },
    ]);
  });

  it('affiche les invitations en attente par défaut', () => {
    expect(DEFAULT_INVITATION_STATUS).toBe('Pending');
  });
});

describe('canResendInvitation', () => {
  it.each(['Pending', 'Expired', 'Revoked'] as const)(
    'autorise le renvoi au statut %s',
    (status) => {
      expect(canResendInvitation(status)).toBe(true);
    }
  );

  it('refuse le renvoi d’une invitation acceptée', () => {
    expect(canResendInvitation('Accepted')).toBe(false);
  });
});

describe('canRevokeInvitation', () => {
  it.each(['Pending', 'Expired'] as const)('autorise la révocation au statut %s', (status) => {
    expect(canRevokeInvitation(status)).toBe(true);
  });

  it.each(['Revoked', 'Accepted'] as const)('refuse la révocation au statut %s', (status) => {
    expect(canRevokeInvitation(status)).toBe(false);
  });
});

describe('messages', () => {
  it('annonce le renvoi avec l’email de l’invitation', () => {
    expect(resendSuccessMessage('marie@exemple.com')).toBe(
      'Invitation renvoyée à marie@exemple.com.'
    );
  });

  it('demande confirmation de la révocation avec l’email de l’invitation', () => {
    expect(revokeConfirmationMessage('marie@exemple.com')).toBe(
      "Révoquer l'invitation envoyée à marie@exemple.com ?"
    );
  });
});
