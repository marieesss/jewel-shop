import { api } from './client';

/* ── Couleurs produit (enum back ProductColor : Gold=0, Silver=1) ──
   L'API ne dispose pas de JsonStringEnumConverter : la couleur doit être
   envoyée en valeur numérique, mais elle est renvoyée en chaîne dans les DTO. */
export const PRODUCT_COLORS = ['Gold', 'Silver'] as const;
export type ProductColor = (typeof PRODUCT_COLORS)[number];

const COLOR_TO_VALUE: Record<ProductColor, number> = { Gold: 0, Silver: 1 };

export const PRODUCT_COLOR_LABELS: Record<ProductColor, string> = {
  Gold: 'Or',
  Silver: 'Argent',
};

/** Libellé FR d'une couleur renvoyée par l'API (« Gold »/« Silver »). */
export function colorLabel(color: string): string {
  return PRODUCT_COLOR_LABELS[color as ProductColor] ?? color;
}

/** Tonalité de badge associée à une couleur produit. */
export function colorTone(color: string): 'gold' | 'silver' | 'neutral' {
  if (color === 'Gold') return 'gold';
  if (color === 'Silver') return 'silver';
  return 'neutral';
}

/* ── Pagination (miroir de PagedResult<T> côté back) ── */
export interface PagedResult<T> {
  items: T[];
  page: number;
  pageSize: number;
  totalCount: number;
  totalPages: number;
  hasPrevious: boolean;
  hasNext: boolean;
}

export interface PageParams {
  page?: number;
  pageSize?: number;
}

/* ── Charm (breloque) ── */
export interface CharmDto {
  id: number;
  name: string;
  description: string;
  color: string;
  imageUrl: string | null;
  url: string | null;
  cost: number;
  price: number;
  stock: number;
}

export interface CreateCharmPayload {
  name: string;
  description: string;
  color: ProductColor;
  cost: number;
  price: number;
  stock: number;
  url?: string | null;
}

/** POST /api/charms (Admin) → 201 CharmDto */
export async function createCharm(payload: CreateCharmPayload): Promise<CharmDto> {
  const { data } = await api.post<CharmDto>('/api/charms', {
    ...payload,
    color: COLOR_TO_VALUE[payload.color],
  });
  return data;
}

/** GET /api/charms → liste paginée */
export async function getCharms(params: PageParams = {}): Promise<PagedResult<CharmDto>> {
  const { data } = await api.get<PagedResult<CharmDto>>('/api/charms', { params });
  return data;
}

/** GET /api/charms/{id} → détail */
export async function getCharmById(id: number): Promise<CharmDto> {
  const { data } = await api.get<CharmDto>(`/api/charms/${id}`);
  return data;
}

/** PUT /api/charms/{id} (Admin) → CharmDto */
export async function updateCharm(id: number, payload: CreateCharmPayload): Promise<CharmDto> {
  const { data } = await api.put<CharmDto>(`/api/charms/${id}`, {
    ...payload,
    color: COLOR_TO_VALUE[payload.color],
  });
  return data;
}

/** POST /api/charms/{id}/image (Admin) → { imageUrl } */
export async function uploadCharmImage(id: number, file: File): Promise<string> {
  return uploadProductImage(`/api/charms/${id}/image`, file);
}

/* ── Chain (chaîne) ── */
export interface ChainDto {
  id: number;
  name: string;
  description: string;
  color: string;
  imageUrl: string | null;
  url: string | null;
  cost: number;
  price: number;
  length: number;
}

export interface CreateChainPayload {
  name: string;
  description: string;
  color: ProductColor;
  cost: number;
  price: number;
  length: number;
  url?: string | null;
}

/** POST /api/chains (Admin) → 201 ChainDto */
export async function createChain(payload: CreateChainPayload): Promise<ChainDto> {
  const { data } = await api.post<ChainDto>('/api/chains', {
    ...payload,
    color: COLOR_TO_VALUE[payload.color],
  });
  return data;
}

/** GET /api/chains → liste paginée */
export async function getChains(params: PageParams = {}): Promise<PagedResult<ChainDto>> {
  const { data } = await api.get<PagedResult<ChainDto>>('/api/chains', { params });
  return data;
}

/** GET /api/chains/{id} → détail */
export async function getChainById(id: number): Promise<ChainDto> {
  const { data } = await api.get<ChainDto>(`/api/chains/${id}`);
  return data;
}

/** PUT /api/chains/{id} (Admin) → ChainDto */
export async function updateChain(id: number, payload: CreateChainPayload): Promise<ChainDto> {
  const { data } = await api.put<ChainDto>(`/api/chains/${id}`, {
    ...payload,
    color: COLOR_TO_VALUE[payload.color],
  });
  return data;
}

/** POST /api/chains/{id}/image (Admin) → { imageUrl } */
export async function uploadChainImage(id: number, file: File): Promise<string> {
  return uploadProductImage(`/api/chains/${id}/image`, file);
}

/* ── Upload d'image (commun breloque/chaîne, multipart champ « file ») ── */
async function uploadProductImage(url: string, file: File): Promise<string> {
  const form = new FormData();
  form.append('file', file);
  const { data } = await api.post<{ imageUrl: string }>(url, form, {
    headers: { 'Content-Type': 'multipart/form-data' },
  });
  return data.imageUrl;
}
