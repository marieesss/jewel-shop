import { AxiosError } from 'axios';

export const FORGOT_PASSWORD_SENT_MESSAGE =
  'Si un compte existe pour cet email, vous recevrez un lien de réinitialisation.';

// Identique à PasswordResetTokenDefaults.InvalidTokenMessage côté API.
export const INVALID_RESET_TOKEN_MESSAGE = 'Ce lien de réinitialisation est invalide ou a expiré.';

export const PASSWORD_MISMATCH_MESSAGE = 'Les mots de passe ne correspondent pas.';

export const PASSWORD_RESET_SUCCESS_MESSAGE =
  'Votre mot de passe a été modifié. Vous pouvez vous reconnecter.';

const BAD_REQUEST = 400;

// Les clés de `errors` sont les noms de propriétés C# (`Token`) : la politique camelCase de l'API ne s'applique pas aux clés de dictionnaire.
const TOKEN_ERROR_KEY = 'token';

/** True si l'API refuse la réinitialisation à cause du lien (token vide, inconnu, utilisé ou expiré). */
export function isInvalidResetTokenError(err: unknown): boolean {
  if (!(err instanceof AxiosError) || err.response?.status !== BAD_REQUEST) return false;

  const errors = (err.response.data as { errors?: Record<string, string[]> | null } | undefined)
    ?.errors;
  return Object.keys(errors ?? {}).some((key) => key.toLowerCase() === TOKEN_ERROR_KEY);
}
