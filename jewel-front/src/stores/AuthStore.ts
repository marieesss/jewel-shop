import { makeAutoObservable, runInAction } from 'mobx';
import {
  login as apiLogin,
  register as apiRegister,
  type LoginPayload,
  type RegisterPayload,
} from '../api/auth';
import { TOKEN_KEY } from '../api/client';
import { extractApiError } from '../api/errors';
import { decodeJwt, isTokenValid } from '../lib/jwt';

const ADMIN_ROLE = 'Admin';

export class AuthStore {
  token: string | null = null;
  userId: number | null = null;
  role: string | null = null;

  loading = false;
  error: string | null = null;

  constructor() {
    makeAutoObservable(this);
    this.hydrate();
  }

  get isAuthenticated(): boolean {
    return this.token !== null;
  }

  get isAdmin(): boolean {
    return this.role === ADMIN_ROLE;
  }

  async login(payload: LoginPayload): Promise<boolean> {
    return this.run(() => apiLogin(payload));
  }

  async register(payload: RegisterPayload): Promise<boolean> {
    return this.run(() => apiRegister(payload));
  }

  logout(): void {
    this.token = null;
    this.userId = null;
    this.role = null;
    this.error = null;
    localStorage.removeItem(TOKEN_KEY);
  }

  clearError(): void {
    this.error = null;
  }

  /** Restaure la session depuis le token persisté (rôle/identité, expiration). */
  private hydrate(): void {
    const stored = localStorage.getItem(TOKEN_KEY);
    if (!stored) return;

    if (!isTokenValid(stored)) {
      localStorage.removeItem(TOKEN_KEY);
      return;
    }

    const claims = decodeJwt(stored);
    this.token = stored;
    this.userId = claims?.userId ?? null;
    this.role = claims?.role ?? null;
  }

  /** Exécute une requête d'auth et applique la réponse, en gérant loading/error. */
  private async run(
    request: () => Promise<{ token: string; userId: number; role: string }>
  ): Promise<boolean> {
    this.loading = true;
    this.error = null;
    try {
      const res = await request();
      runInAction(() => {
        this.token = res.token;
        this.userId = res.userId;
        this.role = res.role;
        localStorage.setItem(TOKEN_KEY, res.token);
      });
      return true;
    } catch (err) {
      runInAction(() => {
        this.error = extractApiError(err, 'Email ou mot de passe incorrect.');
      });
      return false;
    } finally {
      runInAction(() => {
        this.loading = false;
      });
    }
  }
}
