import { AxiosError, type InternalAxiosRequestConfig } from 'axios';
import { beforeEach, describe, expect, it, vi } from 'vitest';
import { addFavorite, type FavoriteDto } from '../api/favorites';
import { FavoritesStore } from './FavoritesStore';

vi.mock('../api/favorites', () => ({
  getMyFavorites: vi.fn(),
  addFavorite: vi.fn(),
  removeFavorite: vi.fn(),
}));

const ALREADY_FAVORITE = 'Ce produit est déjà dans vos favoris.';

function conflict(): AxiosError {
  const config = { headers: {} } as InternalAxiosRequestConfig;
  return new AxiosError('Conflict', 'ERR_BAD_REQUEST', config, null, {
    status: 409,
    statusText: 'Conflict',
    data: { status: 409, message: ALREADY_FAVORITE },
    headers: {},
    config,
  });
}

describe('FavoritesStore', () => {
  beforeEach(() => {
    vi.resetAllMocks();
  });

  it("expose le message de l'API quand l'ajout d'un favori échoue", async () => {
    vi.mocked(addFavorite).mockRejectedValue(conflict());
    const store = new FavoritesStore();

    await store.toggle('charm', 3);

    expect(store.error).toBe(ALREADY_FAVORITE);
    expect(store.isPending('charm', 3)).toBe(false);
  });

  it("efface l'erreur au clic suivant sur un bouton favori", async () => {
    vi.mocked(addFavorite).mockRejectedValueOnce(conflict());
    vi.mocked(addFavorite).mockResolvedValueOnce({ id: 1, chainId: 5 } as FavoriteDto);
    const store = new FavoritesStore();
    await store.toggle('charm', 3);

    await store.toggle('chain', 5);

    expect(store.error).toBeNull();
    expect(store.isFavorite('chain', 5)).toBe(true);
  });

  it("efface l'erreur sur demande, sans toucher aux favoris", async () => {
    vi.mocked(addFavorite).mockResolvedValueOnce({ id: 1, charmId: 3 } as FavoriteDto);
    vi.mocked(addFavorite).mockRejectedValueOnce(conflict());
    const store = new FavoritesStore();
    await store.toggle('charm', 3);
    await store.toggle('charm', 4);

    store.clearError();

    expect(store.error).toBeNull();
    expect(store.isFavorite('charm', 3)).toBe(true);
  });
});
