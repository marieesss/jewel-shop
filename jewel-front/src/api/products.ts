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
