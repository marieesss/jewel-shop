import { AxiosError, type AxiosInstance, type InternalAxiosRequestConfig } from 'axios';

const UNAUTHORIZED = 401;

// Un 401 sur ces routes est une réponse métier (identifiants faux, session expirée), pas un access token expiré.
const ROUTES_WITHOUT_REFRESH = [
  '/api/auth/login',
  '/api/auth/register',
  '/api/auth/refresh',
  '/api/auth/logout',
];

export interface AuthInterceptorOptions {
  getAccessToken: () => string | null;
  /** Renouvelle la session ; rejette si elle a expiré. */
  refreshAccessToken: () => Promise<unknown>;
  onRefreshFailed: () => void;
}

interface ReplayableRequestConfig extends InternalAxiosRequestConfig {
  _replayed?: boolean;
}

function bearer(token: string): string {
  return `Bearer ${token}`;
}

function isRouteWithoutRefresh(url: string | undefined): boolean {
  return ROUTES_WITHOUT_REFRESH.some((route) => url?.startsWith(route));
}

/** Requête à rejouer après un refresh, ou `null` si l'erreur ne relève pas d'un access token expiré. */
function replayableRequestOf(error: unknown): ReplayableRequestConfig | null {
  if (!(error instanceof AxiosError) || error.response?.status !== UNAUTHORIZED) return null;

  const config = error.config as ReplayableRequestConfig | undefined;
  if (!config || config._replayed || isRouteWithoutRefresh(config.url)) return null;
  return config;
}

/**
 * Injecte l'access token sur chaque requête et, sur un 401, renouvelle la session
 * puis rejoue la requête une seule fois.
 */
export function installAuthInterceptors(
  instance: AxiosInstance,
  { getAccessToken, refreshAccessToken, onRefreshFailed }: AuthInterceptorOptions
): void {
  instance.interceptors.request.use((config) => {
    const token = getAccessToken();
    if (token) {
      config.headers.Authorization = bearer(token);
    }
    return config;
  });

  instance.interceptors.response.use(undefined, async (error: unknown) => {
    const request = replayableRequestOf(error);
    if (!request) throw error;

    request._replayed = true;
    const currentToken = getAccessToken();
    const tokenRenewedSinceSent =
      currentToken !== null && request.headers.Authorization !== bearer(currentToken);

    if (!tokenRenewedSinceSent) {
      try {
        await refreshAccessToken();
      } catch {
        onRefreshFailed();
        throw error;
      }
    }
    return instance(request);
  });
}
