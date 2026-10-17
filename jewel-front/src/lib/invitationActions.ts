import type { InvitationStatus } from '../api/admin';
import type { SelectOption } from '../components/ui/Select';

export const DEFAULT_INVITATION_STATUS: InvitationStatus = 'Pending';

export const INVITATION_STATUS_OPTIONS: (SelectOption & { value: InvitationStatus })[] = [
  { value: 'Pending', label: 'En attente' },
  { value: 'Expired', label: 'Expirées' },
  { value: 'Revoked', label: 'Révoquées' },
  { value: 'Accepted', label: 'Acceptées' },
];

// Reprend les statuts acceptés par les handlers de renvoi et de révocation côté API.
const RESENDABLE_STATUSES: readonly InvitationStatus[] = ['Pending', 'Expired', 'Revoked'];
const REVOCABLE_STATUSES: readonly InvitationStatus[] = ['Pending', 'Expired'];

export const REVOKE_SUCCESS_MESSAGE = 'Invitation révoquée.';

export function canResendInvitation(status: InvitationStatus): boolean {
  return RESENDABLE_STATUSES.includes(status);
}

export function canRevokeInvitation(status: InvitationStatus): boolean {
  return REVOCABLE_STATUSES.includes(status);
}

export function resendSuccessMessage(email: string): string {
  return `Invitation renvoyée à ${email}.`;
}

export function revokeConfirmationMessage(email: string): string {
  return `Révoquer l'invitation envoyée à ${email} ?`;
}
