/** État de navigation qui transmet un message de confirmation à la page de connexion. */
export interface LoginMessageState {
  message?: string;
}

/** Message de confirmation à afficher sur la page de connexion, ou `null` s'il n'y en a pas. */
export function loginMessageOf(state: unknown): string | null {
  if (typeof state !== 'object' || state === null) return null;

  const { message } = state as LoginMessageState;
  return typeof message === 'string' && message !== '' ? message : null;
}
