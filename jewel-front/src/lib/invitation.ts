import { AxiosError } from 'axios';

// Reprend InvitationDefaults.InvalidInvitationMessage côté API, complété d'une consigne pour l'utilisateur.
export const INVALID_INVITATION_MESSAGE =
  "Cette invitation est invalide ou a expiré. Demandez une nouvelle invitation à l'administrateur.";

const BAD_REQUEST = 400;
const NOT_FOUND = 404;

// Les clés de `errors` sont les noms de propriétés C# (`InvitationToken`) : la politique camelCase de l'API ne s'applique pas aux clés de dictionnaire.
const INVITATION_TOKEN_ERROR_KEY = 'invitationtoken';

/** True si l'API ne trouve pas d'invitation en attente pour ce token (inconnue, expirée, révoquée ou acceptée). */
export function isInvitationNotFoundError(err: unknown): boolean {
  return err instanceof AxiosError && err.response?.status === NOT_FOUND;
}

/** True si l'API refuse l'inscription à cause de l'invitation. */
export function isInvalidInvitationError(err: unknown): boolean {
  if (!(err instanceof AxiosError) || err.response?.status !== BAD_REQUEST) return false;

  const errors = (err.response.data as { errors?: Record<string, string[]> | null } | undefined)
    ?.errors;
  return Object.keys(errors ?? {}).some((key) => key.toLowerCase() === INVITATION_TOKEN_ERROR_KEY);
}
