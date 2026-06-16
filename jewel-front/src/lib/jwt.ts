/**
 * Décodage (sans vérification de signature) d'un JWT pour lire ses claims côté
 * client : restaurer l'identité/le rôle au rechargement et détecter l'expiration.
 * La vérification cryptographique reste la responsabilité du back-end.
 */

// Claim de rôle émis par ASP.NET (ClaimTypes.Role).
const ROLE_CLAIM = 'http://schemas.microsoft.com/ws/2008/06/identity/claims/role';

export interface JwtClaims {
  userId: number | null;
  role: string | null;
  email: string | null;
  /** Date d'expiration (timestamp ms), ou null si absente. */
  expiresAt: number | null;
}

function base64UrlDecode(segment: string): string {
  const base64 = segment.replace(/-/g, '+').replace(/_/g, '/');
  const padded = base64.padEnd(base64.length + ((4 - (base64.length % 4)) % 4), '=');
  const binary = atob(padded);
  // Gère correctement l'UTF-8.
  const bytes = Uint8Array.from(binary, (c) => c.charCodeAt(0));
  return new TextDecoder().decode(bytes);
}

export function decodeJwt(token: string): JwtClaims | null {
  const parts = token.split('.');
  if (parts.length !== 3) return null;

  try {
    const payload = JSON.parse(base64UrlDecode(parts[1])) as Record<string, unknown>;

    const sub = payload.sub ?? payload.nameid;
    const role = payload[ROLE_CLAIM] ?? payload.role;
    const exp = typeof payload.exp === 'number' ? payload.exp : null;

    return {
      userId: sub != null ? Number(sub) : null,
      role: typeof role === 'string' ? role : null,
      email: typeof payload.email === 'string' ? payload.email : null,
      expiresAt: exp != null ? exp * 1000 : null,
    };
  } catch {
    return null;
  }
}

/** True si le token est décodable et non expiré. */
export function isTokenValid(token: string): boolean {
  const claims = decodeJwt(token);
  if (!claims) return false;
  if (claims.expiresAt != null && claims.expiresAt <= Date.now()) return false;
  return true;
}
