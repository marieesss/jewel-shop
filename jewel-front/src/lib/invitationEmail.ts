// Reproduit CreateInvitationCommandValidator côté API : mêmes règles, mêmes messages.
import { validateEmail } from './emailValidation';

export const INVITATION_EMAIL_MAX_LENGTH = 255;
const MAX_LENGTH_MESSAGE = `${INVITATION_EMAIL_MAX_LENGTH} caractères maximum.`;

/** Messages des règles non respectées, dans l'ordre du back. Tableau vide si l'email est valide. */
export function validateInvitationEmail(email: string): string[] {
  const isTooLong = email.length > INVITATION_EMAIL_MAX_LENGTH;
  return [...validateEmail(email), ...(isTooLong ? [MAX_LENGTH_MESSAGE] : [])];
}
