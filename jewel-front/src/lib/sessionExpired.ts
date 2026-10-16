export const SESSION_EXPIRED_MESSAGE = 'Votre session a expiré. Veuillez vous reconnecter.';

/** État de navigation transmis à la page de connexion. */
export interface LoginLocationState {
  sessionExpired?: boolean;
}

/** True si la redirection vers la connexion fait suite à l'expiration de la session. */
export function isSessionExpiredState(state: unknown): boolean {
  return (
    typeof state === 'object' &&
    state !== null &&
    (state as LoginLocationState).sessionExpired === true
  );
}
