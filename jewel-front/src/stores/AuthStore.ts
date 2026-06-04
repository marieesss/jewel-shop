import { makeAutoObservable, runInAction } from 'mobx';
import { AxiosError } from 'axios';
import {
  login as apiLogin,
  register as apiRegister,
  type LoginPayload,
  type RegisterPayload,
} from '../api/auth';
import { TOKEN_KEY } from '../api/client';

export class AuthStore {
  token: string | null = localStorage.getItem(TOKEN_KEY);
  userId: number | null = null;
  role: string | null = null;

  loading = false;
  error: string | null = null;

  constructor() {
    makeAutoObservable(this);
  }

  get isAuthenticated(): boolean {
    return this.token !== null;
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
        this.error = extractError(err);
      });
      return false;
    } finally {
      runInAction(() => {
        this.loading = false;
      });
    }
  }
}

/** Transforme une erreur Axios en message lisible pour l'utilisateur. */
function extractError(err: unknown): string {
  if (err instanceof AxiosError) {
    const data = err.response?.data as
      | { message?: string; title?: string; errors?: Record<string, string[]> }
      | undefined;

    if (data?.errors) {
      const first = Object.values(data.errors).flat()[0];
      if (first) return first;
    }
    if (data?.message) return data.message;
    if (data?.title) return data.title;

    if (err.response?.status === 401) return 'Email ou mot de passe incorrect.';
    if (err.response?.status === 409) return 'Un compte existe déjà avec cet email.';
    if (!err.response) return 'Impossible de joindre le serveur.';
  }
  return 'Une erreur est survenue. Veuillez réessayer.';
}
