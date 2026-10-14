const LOGIN_PATH = '/login';
const ADMIN_PATH = '/admin';
const USER_HOME_PATH = '/';

/** Emplacement demandé, tel que fourni par `useLocation()`. */
interface RequestedLocation {
  pathname: string;
  search?: string;
  hash?: string;
}

/** Page de connexion qui renverra ensuite sur l'emplacement demandé. */
export function loginPathFor({ pathname, search = '', hash = '' }: RequestedLocation): string {
  return `${LOGIN_PATH}?returnUrl=${encodeURIComponent(pathname + search + hash)}`;
}

/**
 * Retourne le `returnUrl` s'il désigne un chemin interne à l'application, sinon `null`.
 * `//hote` et `/\hote` sont interprétés par les navigateurs comme des URL externes.
 */
export function safeReturnUrl(raw: string | null): string | null {
  if (!raw) return null;
  const isInternalPath =
    raw.startsWith('/') && !raw.startsWith('//') && !raw.startsWith('/\\') && !raw.includes('://');
  return isInternalPath ? raw : null;
}

/** Destination après connexion : le `returnUrl` s'il est sûr et autorisé pour le rôle, sinon l'accueil du rôle. */
export function postLoginPath(rawReturnUrl: string | null, isAdmin: boolean): string {
  const defaultPath = isAdmin ? ADMIN_PATH : USER_HOME_PATH;
  const returnUrl = safeReturnUrl(rawReturnUrl);
  if (!returnUrl) return defaultPath;

  const isAdminArea = returnUrl.startsWith(ADMIN_PATH);
  if (isAdminArea && !isAdmin) return USER_HOME_PATH;
  return returnUrl;
}
