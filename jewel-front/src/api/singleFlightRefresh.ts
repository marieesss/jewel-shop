import type { AuthResponse } from './auth';

export const REFRESH_LOCK_NAME = 'jewelryshop-refresh';

/** Partie de la Web Locks API utilisée ici, remplaçable en test. */
export type RefreshLocks = Pick<LockManager, 'request'>;

function navigatorLocks(): RefreshLocks | null {
  return globalThis.navigator?.locks ?? null;
}

/**
 * Garantit un seul refresh à la fois. Dans un onglet, les appels concurrents partagent la même
 * promesse. Entre onglets, le verrou Web Locks les sérialise : le cookie étant partagé, l'onglet
 * qui obtient le verrou en second présente le cookie déjà renouvelé par le premier, au lieu d'un
 * token tourné que le back prendrait pour une réutilisation.
 */
export function createSingleFlightRefresh(
  refresh: () => Promise<AuthResponse>,
  locks: RefreshLocks | null = navigatorLocks()
): () => Promise<AuthResponse> {
  let pending: Promise<AuthResponse> | null = null;

  const refreshExclusively = async (): Promise<AuthResponse> =>
    locks ? await locks.request(REFRESH_LOCK_NAME, () => refresh()) : refresh();

  return () => {
    pending ??= refreshExclusively().finally(() => {
      pending = null;
    });
    return pending;
  };
}
