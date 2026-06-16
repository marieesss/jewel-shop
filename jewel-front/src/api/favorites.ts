import { api } from './client';
import type { CharmDto, ChainDto, PagedResult, PageParams } from './products';
import type { ProductKind } from '../stores/ProductAdminStore';

export interface FavoriteDto {
  id: number;
  userId: number;
  chainId: number | null;
  charmId: number | null;
  chain: ChainDto | null;
  charm: CharmDto | null;
}

/** GET /api/favorites → mes favoris paginés. */
export async function getMyFavorites(params: PageParams = {}): Promise<PagedResult<FavoriteDto>> {
  const { data } = await api.get<PagedResult<FavoriteDto>>('/api/favorites', { params });
  return data;
}

/** POST /api/favorites → ajoute un produit (chainId OU charmId). */
export async function addFavorite(kind: ProductKind, productId: number): Promise<FavoriteDto> {
  const body = kind === 'charm' ? { charmId: productId } : { chainId: productId };
  const { data } = await api.post<FavoriteDto>('/api/favorites', body);
  return data;
}

/** DELETE /api/favorites/{id} → retire un favori. */
export async function removeFavorite(favoriteId: number): Promise<void> {
  await api.delete(`/api/favorites/${favoriteId}`);
}
