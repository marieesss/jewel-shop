import { describe, expect, it } from 'vitest';
import { loginPathFor, postLoginPath, safeReturnUrl } from './returnUrl';

function returnUrlOf(loginPath: string): string | null {
  return new URLSearchParams(loginPath.split('?')[1]).get('returnUrl');
}

describe('loginPathFor', () => {
  it('encode le chemin demandé dans le returnUrl', () => {
    expect(loginPathFor({ pathname: '/chains' })).toBe('/login?returnUrl=%2Fchains');
  });

  it('conserve la query string et le hash du chemin demandé', () => {
    const loginPath = loginPathFor({ pathname: '/charms/12', search: '?x=1', hash: '#avis' });

    expect(returnUrlOf(loginPath)).toBe('/charms/12?x=1#avis');
  });
});

describe('safeReturnUrl', () => {
  it.each(['/chains', '/charms/12?x=1', '/admin/products/charms'])(
    'accepte le chemin interne %s',
    (path) => {
      expect(safeReturnUrl(path)).toBe(path);
    }
  );

  it.each([
    ['une URL absolue', 'https://exemple.com'],
    ['une URL sans protocole', '//exemple.com'],
    ['une URL avec antislash', '/\\exemple.com'],
    ['un chemin contenant un protocole', '/chains?next=https://exemple.com'],
    ['un chemin relatif', 'chains'],
    ['une valeur vide', ''],
    ['une valeur absente', null],
  ])('refuse %s', (_cas, raw) => {
    expect(safeReturnUrl(raw)).toBeNull();
  });
});

describe('postLoginPath', () => {
  it('renvoie un utilisateur sur la page demandée', () => {
    expect(postLoginPath('/chains', false)).toBe('/chains');
  });

  it('renvoie sur la page demandée avec sa query string', () => {
    const loginPath = loginPathFor({ pathname: '/charms/12', search: '?x=1' });

    expect(postLoginPath(returnUrlOf(loginPath), false)).toBe('/charms/12?x=1');
  });

  it.each([
    ['un utilisateur', false, '/'],
    ['un admin', true, '/admin'],
  ])(
    'envoie %s sur sa destination par défaut si le returnUrl est externe',
    (_cas, isAdmin, expected) => {
      expect(postLoginPath('https://exemple.com', isAdmin)).toBe(expected);
      expect(postLoginPath('//exemple.com', isAdmin)).toBe(expected);
    }
  );

  it.each(['/admin', '/admin/products/charms'])(
    "envoie un utilisateur sur l'accueil quand le returnUrl %s est réservé aux admins",
    (returnUrl) => {
      expect(postLoginPath(returnUrl, false)).toBe('/');
    }
  );

  it('renvoie un admin sur la page admin demandée', () => {
    expect(postLoginPath('/admin/products/charms', true)).toBe('/admin/products/charms');
  });

  it.each([
    ['un utilisateur', false, '/'],
    ['un admin', true, '/admin'],
  ])('envoie %s sur sa destination par défaut sans returnUrl', (_cas, isAdmin, expected) => {
    expect(postLoginPath(null, isAdmin)).toBe(expected);
  });
});
