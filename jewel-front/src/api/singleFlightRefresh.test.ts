import { describe, expect, it, vi } from 'vitest';
import type { AuthResponse } from './auth';
import {
  REFRESH_LOCK_NAME,
  createSingleFlightRefresh,
  type RefreshLocks,
} from './singleFlightRefresh';

const session: AuthResponse = { token: 'nouveau', userId: 1, role: 'User' };

function deferred<T>() {
  let resolve!: (value: T) => void;
  const promise = new Promise<T>((r) => (resolve = r));
  return { promise, resolve };
}

describe('createSingleFlightRefresh', () => {
  it('partage le même refresh entre des appels concurrents', async () => {
    const pending = deferred<AuthResponse>();
    const refresh = vi.fn(() => pending.promise);
    const refreshOnce = createSingleFlightRefresh(refresh, null);

    const calls = [refreshOnce(), refreshOnce(), refreshOnce()];
    pending.resolve(session);

    await expect(Promise.all(calls)).resolves.toEqual([session, session, session]);
    expect(refresh).toHaveBeenCalledTimes(1);
  });

  it('relance un refresh une fois le précédent terminé', async () => {
    const refresh = vi.fn(() => Promise.resolve(session));
    const refreshOnce = createSingleFlightRefresh(refresh, null);

    await refreshOnce();
    await refreshOnce();

    expect(refresh).toHaveBeenCalledTimes(2);
  });

  it('relance un refresh après un échec', async () => {
    const refresh = vi
      .fn<() => Promise<AuthResponse>>()
      .mockRejectedValueOnce(new Error('expiré'))
      .mockResolvedValueOnce(session);
    const refreshOnce = createSingleFlightRefresh(refresh, null);

    await expect(refreshOnce()).rejects.toThrow('expiré');
    await expect(refreshOnce()).resolves.toEqual(session);
  });

  it('exécute le refresh sous le verrou partagé entre onglets', async () => {
    const request = vi.fn((_name: string, callback: () => Promise<AuthResponse>) => callback());
    const locks = { request } as unknown as RefreshLocks;
    const refresh = vi.fn(() => Promise.resolve(session));

    await createSingleFlightRefresh(refresh, locks)();

    expect(request).toHaveBeenCalledWith(REFRESH_LOCK_NAME, expect.any(Function));
    expect(refresh).toHaveBeenCalledTimes(1);
  });

  it('exécute le refresh sans verrou si la Web Locks API est absente', async () => {
    const refresh = vi.fn(() => Promise.resolve(session));

    await expect(createSingleFlightRefresh(refresh, null)()).resolves.toEqual(session);
  });
});
