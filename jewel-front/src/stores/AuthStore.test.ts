import { AxiosError, AxiosHeaders, type AxiosResponse } from 'axios';
import { beforeEach, describe, expect, it, vi } from 'vitest';
import { login, logout, refresh, register, type AuthResponse } from '../api/auth';
import { AuthStore } from './AuthStore';

vi.mock('../api/auth', () => ({
  login: vi.fn(),
  logout: vi.fn(),
  refresh: vi.fn(),
  register: vi.fn(),
}));

const session: AuthResponse = { token: 'access', userId: 7, role: 'Admin' };

const registerPayload = {
  name: 'Marie',
  surname: 'Dupont',
  email: 'marie@exemple.com',
  password: 'MotDePasse1!',
  invitationToken: 'invitation',
};

function apiError(status: number, errors: Record<string, string[]>): AxiosError {
  const response = {
    status,
    data: { status, title: 'Erreur', errors },
    statusText: '',
    headers: {},
    config: { headers: new AxiosHeaders() },
  } as AxiosResponse;
  return new AxiosError('Erreur', undefined, undefined, undefined, response);
}

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
    expect(store.sessionExpired).toBe(true);
  });

  it("n'annonce pas d'expiration quand le refresh échoue au chargement", async () => {
    vi.mocked(refresh).mockRejectedValue(new Error('401'));
    const store = new AuthStore();

    await store.restoreSession();

    expect(store.sessionExpired).toBe(false);
  });

  it("n'annonce plus l'expiration après une nouvelle connexion", async () => {
    vi.mocked(login).mockResolvedValue(session);
    const store = connectedStore();
    store.expireSession();

    await store.login({ email: 'a@b.fr', password: 'secret' });

    expect(store.sessionExpired).toBe(false);
  });

  it("n'annonce pas d'expiration après une déconnexion volontaire", async () => {
    vi.mocked(logout).mockResolvedValue();
    const store = connectedStore();
    store.expireSession();

    await store.logout();

    expect(store.sessionExpired).toBe(false);
  });

  it('applique la session renvoyée par la connexion', async () => {
    vi.mocked(login).mockResolvedValue(session);
    const store = new AuthStore();

    const ok = await store.login({ email: 'a@b.fr', password: 'secret' });

    expect(ok).toBe(true);
    expect(store.token).toBe('access');
  });

  it('applique la session renvoyée par l’inscription', async () => {
    vi.mocked(register).mockResolvedValue(session);
    const store = new AuthStore();

    const outcome = await store.register(registerPayload);

    expect(outcome).toBe('success');
    expect(store.token).toBe('access');
    expect(register).toHaveBeenCalledWith(registerPayload);
  });

  it('signale une invitation refusée, sans message d’erreur générique', async () => {
    vi.mocked(register).mockRejectedValue(
      apiError(400, { InvitationToken: ['Cette invitation est invalide ou a expiré.'] })
    );
    const store = new AuthStore();

    const outcome = await store.register(registerPayload);

    expect(outcome).toBe('invalid-invitation');
    expect(store.error).toBeNull();
    expect(store.isAuthenticated).toBe(false);
  });

  it('expose le message de l’API quand l’inscription échoue pour une autre raison', async () => {
    vi.mocked(register).mockRejectedValue(
      apiError(409, { Email: ['Un compte existe déjà pour cet email.'] })
    );
    const store = new AuthStore();

    const outcome = await store.register(registerPayload);

    expect(outcome).toBe('error');
    expect(store.error).not.toBeNull();
    expect(store.isAuthenticated).toBe(false);
  });
});
