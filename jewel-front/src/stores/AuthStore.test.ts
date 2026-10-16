import { beforeEach, describe, expect, it, vi } from 'vitest';
import { login, logout, refresh, type AuthResponse } from '../api/auth';
import { AuthStore } from './AuthStore';

vi.mock('../api/auth', () => ({
  login: vi.fn(),
  logout: vi.fn(),
  refresh: vi.fn(),
  register: vi.fn(),
}));

const session: AuthResponse = { token: 'access', userId: 7, role: 'Admin' };

function connectedStore(): AuthStore {
  const store = new AuthStore();
  store.token = session.token;
  store.userId = session.userId;
  store.role = session.role;
  return store;
}

describe('AuthStore', () => {
  const removeItem = vi.fn();

  beforeEach(() => {
    vi.resetAllMocks();
    vi.stubGlobal('localStorage', { removeItem });
  });

  it("supprime l'ancien token du localStorage au démarrage", () => {
    new AuthStore();

    expect(removeItem).toHaveBeenCalledWith('jewelryshop_token');
  });

  it('restaure la session au chargement quand le refresh réussit', async () => {
    vi.mocked(refresh).mockResolvedValue(session);
    const store = new AuthStore();

    await store.restoreSession();

    expect(store.isAuthenticated).toBe(true);
    expect(store.isAdmin).toBe(true);
    expect(store.userId).toBe(7);
    expect(store.initializing).toBe(false);
  });

  it('laisse le visiteur déconnecté, sans erreur, quand le refresh échoue au chargement', async () => {
    vi.mocked(refresh).mockRejectedValue(new Error('401'));
    const store = new AuthStore();

    await store.restoreSession();

    expect(store.isAuthenticated).toBe(false);
    expect(store.error).toBeNull();
    expect(store.initializing).toBe(false);
  });

  it('appelle le logout de l’API puis vide la session', async () => {
    vi.mocked(logout).mockResolvedValue();
    const store = connectedStore();

    await store.logout();

    expect(logout).toHaveBeenCalledTimes(1);
    expect(store.isAuthenticated).toBe(false);
    expect(store.role).toBeNull();
  });

  it('vide la session même si le logout de l’API échoue', async () => {
    vi.mocked(logout).mockRejectedValue(new Error('réseau'));
    const store = connectedStore();

    await store.logout();

    expect(store.isAuthenticated).toBe(false);
  });

  it('vide la session quand elle expire', () => {
    const store = connectedStore();

    store.expireSession();

    expect(store.isAuthenticated).toBe(false);
    expect(store.userId).toBeNull();
  });

  it('applique la session renvoyée par la connexion', async () => {
    vi.mocked(login).mockResolvedValue(session);
    const store = new AuthStore();

    const ok = await store.login({ email: 'a@b.fr', password: 'secret' });

    expect(ok).toBe(true);
    expect(store.token).toBe('access');
  });
});
