import { makeAutoObservable, runInAction } from 'mobx';
import {
  login as apiLogin,
  logout as apiLogout,
  refresh as apiRefresh,
  register as apiRegister,
  type AuthResponse,
  type LoginPayload,
  type RegisterPayload,
} from '../api/auth';
import { installAuthInterceptors } from '../api/authInterceptors';
import { api } from '../api/client';
import { extractApiError } from '../api/errors';
import { createSingleFlightRefresh } from '../api/singleFlightRefresh';
import { isInvalidInvitationError } from '../lib/invitation';

const ADMIN_ROLE = 'Admin';

export type RegisterOutcome = 'success' | 'invalid-invitation' | 'error';

type RequestOutcome = { ok: true } | { ok: false; error: unknown };

// Emplacement de l'access token avant son passage en mémoire : nettoyé pour ne pas le laisser dans le navigateur.
const LEGACY_TOKEN_KEY = 'jewelryshop_token';

export class AuthStore {
  token: string | null = null;
  userId: number | null = null;
  role: string | null = null;

  /** Vrai tant que la session n'a pas été restaurée (ou écartée) au chargement de la page. */
  initializing = true;

  /** Vrai quand la session a été fermée par l'échec d'un refresh, pour l'annoncer à la connexion. */
  sessionExpired = false;

  loading = false;
  error: string | null = null;

  private readonly refreshOnce = createSingleFlightRefresh(apiRefresh);

  constructor() {
    makeAutoObservable<AuthStore, 'refreshOnce'>(this, { refreshOnce: false });
    localStorage.removeItem(LEGACY_TOKEN_KEY);
    installAuthInterceptors(api, {
      getAccessToken: () => this.token,
      refreshAccessToken: () => this.refreshSession(),
      onRefreshFailed: () => this.expireSession(),
    });
  }

  get isAuthenticated(): boolean {
    return this.token !== null;
  }

  get isAdmin(): boolean {
    return this.role === ADMIN_ROLE;
  }

  async login(payload: LoginPayload): Promise<boolean> {
    const outcome = await this.run(() => apiLogin(payload));
    return outcome.ok;
  }

  /** Crée le compte ; une invitation refusée est signalée à part, sans message d'erreur générique. */
  async register(payload: RegisterPayload): Promise<RegisterOutcome> {
    const outcome = await this.run(() => apiRegister(payload));
    if (outcome.ok) return 'success';
    if (!isInvalidInvitationError(outcome.error)) return 'error';

    this.clearError();
    return 'invalid-invitation';
  }

  /** Restaure la session à partir du cookie de refresh ; un échec laisse simplement l'utilisateur déconnecté. */
  async restoreSession(): Promise<void> {
    try {
      await this.refreshSession();
    } catch {
      runInAction(() => this.clearSession());
    } finally {
      runInAction(() => {
        this.initializing = false;
      });
    }
  }

  /** Renouvelle l'access token avec le cookie de refresh ; rejette si la session a expiré. */
  async refreshSession(): Promise<void> {
    const session = await this.refreshOnce();
    runInAction(() => this.applySession(session));
  }

  /** Ferme la session après l'échec d'un refresh ; les guards redirigent alors vers la connexion. */
  expireSession(): void {
    this.clearSession();
    this.sessionExpired = true;
  }

  async logout(): Promise<void> {
    // Une erreur réseau ne doit pas empêcher de fermer la session côté front.
    await apiLogout().catch(() => undefined);
    runInAction(() => {
      this.clearSession();
      this.sessionExpired = false;
    });
  }

  clearError(): void {
    this.error = null;
  }

  private applySession(session: AuthResponse): void {
    this.token = session.token;
    this.userId = session.userId;
    this.role = session.role;
    this.sessionExpired = false;
  }

  private clearSession(): void {
    this.token = null;
    this.userId = null;
    this.role = null;
    this.error = null;
  }

  /** Exécute une requête d'auth et applique la réponse, en gérant loading/error. */
  private async run(request: () => Promise<AuthResponse>): Promise<RequestOutcome> {
    this.loading = true;
    this.error = null;
    try {
      const session = await request();
      runInAction(() => this.applySession(session));
      return { ok: true };
    } catch (err) {
      runInAction(() => {
        this.error = extractApiError(err, 'Email ou mot de passe incorrect.');
      });
      return { ok: false, error: err };
    } finally {
      runInAction(() => {
        this.loading = false;
      });
    }
  }
}
