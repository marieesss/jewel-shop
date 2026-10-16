import axios, { AxiosError, type AxiosAdapter, type InternalAxiosRequestConfig } from 'axios';
import { describe, expect, it, vi } from 'vitest';
import { installAuthInterceptors } from './authInterceptors';
import { createSingleFlightRefresh } from './singleFlightRefresh';

const EXPIRED_TOKEN = 'expire';
const RENEWED_TOKEN = 'renouvele';

/**
 * API simulée : accepte seulement le token valide du moment, renvoie 401 sinon.
 * Le refresh remplace le token valide et le token courant du client.
 */
function setup({ refreshFails = false, renewedByAnotherRequest = false } = {}) {
  let accessToken: string | null = EXPIRED_TOKEN;
  const sentAuthorizations: string[] = [];

  const unauthorized = (config: InternalAxiosRequestConfig) =>
    new AxiosError('Unauthorized', 'ERR_BAD_REQUEST', config, null, {
      status: 401,
      statusText: 'Unauthorized',
      data: {},
      headers: {},
      config,
    });

  const adapter: AxiosAdapter = async (config) => {
    const authorization = String(config.headers.Authorization ?? '');
    sentAuthorizations.push(`${config.url} ${authorization}`);
    if (renewedByAnotherRequest) accessToken = RENEWED_TOKEN;
    await Promise.resolve();
    if (authorization !== `Bearer ${RENEWED_TOKEN}`) throw unauthorized(config);
    return { status: 200, statusText: 'OK', data: config.url, headers: {}, config };
  };

  const refresh = vi.fn(async () => {
    await Promise.resolve();
    if (refreshFails) throw new Error('Session expirée');
    accessToken = RENEWED_TOKEN;
    return { token: RENEWED_TOKEN, userId: 1, role: 'User' };
  });
  const onRefreshFailed = vi.fn();
  const instance = axios.create({ adapter });

  installAuthInterceptors(instance, {
    getAccessToken: () => accessToken,
    refreshAccessToken: createSingleFlightRefresh(refresh, null),
    onRefreshFailed,
  });

  return {
    instance,
    refresh,
    onRefreshFailed,
    sentAuthorizations,
    setAccessToken: (t: string) => (accessToken = t),
  };
}

describe('installAuthInterceptors', () => {
  it('injecte le Bearer token courant', async () => {
    const { instance, setAccessToken, refresh } = setup();
    setAccessToken(RENEWED_TOKEN);

    await expect(instance.get('/api/charms')).resolves.toMatchObject({ data: '/api/charms' });
    expect(refresh).not.toHaveBeenCalled();
  });

  it('rafraîchit la session sur 401 puis rejoue la requête avec le nouveau token', async () => {
    const { instance, refresh, sentAuthorizations } = setup();

    const response = await instance.get('/api/favorites');

    expect(response.data).toBe('/api/favorites');
    expect(refresh).toHaveBeenCalledTimes(1);
    expect(sentAuthorizations).toEqual([
      `/api/favorites Bearer ${EXPIRED_TOKEN}`,
      `/api/favorites Bearer ${RENEWED_TOKEN}`,
    ]);
  });

  it('fait un seul refresh pour trois requêtes simultanées en 401 et les rejoue toutes', async () => {
    const { instance, refresh } = setup();

    const responses = await Promise.all([
      instance.get('/api/charms'),
      instance.get('/api/chains'),
      instance.get('/api/favorites'),
    ]);

    expect(responses.map((r) => r.data)).toEqual(['/api/charms', '/api/chains', '/api/favorites']);
    expect(refresh).toHaveBeenCalledTimes(1);
  });

  it('rejoue sans refresh une requête partie avec un token déjà renouvelé', async () => {
    const { instance, refresh, sentAuthorizations } = setup({ renewedByAnotherRequest: true });

    await expect(instance.get('/api/charms')).resolves.toMatchObject({ data: '/api/charms' });
    expect(refresh).not.toHaveBeenCalled();
    expect(sentAuthorizations).toEqual([
      `/api/charms Bearer ${EXPIRED_TOKEN}`,
      `/api/charms Bearer ${RENEWED_TOKEN}`,
    ]);
  });

  it.each(['/api/auth/login', '/api/auth/register', '/api/auth/refresh', '/api/auth/logout'])(
    'ne rafraîchit pas la session sur un 401 de %s',
    async (url) => {
      const { instance, refresh } = setup();

      await expect(instance.post(url)).rejects.toMatchObject({ response: { status: 401 } });
      expect(refresh).not.toHaveBeenCalled();
    }
  );

  it('ne rejoue pas une seconde fois une requête encore refusée après le refresh', async () => {
    const { instance, refresh, setAccessToken, sentAuthorizations } = setup();
    refresh.mockImplementation(async () => {
      setAccessToken('toujours-refuse');
      return { token: 'toujours-refuse', userId: 1, role: 'User' };
    });

    await expect(instance.get('/api/charms')).rejects.toMatchObject({ response: { status: 401 } });
    expect(refresh).toHaveBeenCalledTimes(1);
    expect(sentAuthorizations).toHaveLength(2);
  });

  it("signale l'échec du refresh et renvoie l'erreur 401 d'origine", async () => {
    const { instance, onRefreshFailed } = setup({ refreshFails: true });

    await expect(instance.get('/api/favorites')).rejects.toMatchObject({
      response: { status: 401 },
    });
    expect(onRefreshFailed).toHaveBeenCalledTimes(1);
  });
});
