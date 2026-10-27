import { api } from './client';

/** Catégories de tag, dans l'ordre de tri de l'API (enum back `TagCategory`, envoyée par son nom). */
export const TAG_CATEGORIES = ['Theme', 'Color', 'Material', 'Occasion'] as const;
export type TagCategory = (typeof TAG_CATEGORIES)[number];

export const TAG_CATEGORY_LABELS: Record<TagCategory, string> = {
  Theme: 'Thème',
  Color: 'Couleur',
  Material: 'Matière',
  Occasion: 'Occasion',
};

/** Libellé FR d'une catégorie renvoyée par l'API (« Theme » → « Thème »). */
export function tagCategoryLabel(category: string): string {
  return TAG_CATEGORY_LABELS[category as TagCategory] ?? category;
}

export interface TagDto {
  id: number;
  name: string;
  slug: string;
  category: string;
}

export interface CreateTagPayload {
  name: string;
  category: TagCategory;
}

/** GET /api/tags → tags triés par catégorie puis par nom */
export async function getTags(): Promise<TagDto[]> {
  const { data } = await api.get<TagDto[]>('/api/tags');
  return data;
}

/** POST /api/admin/tags (Admin) → 201 TagDto ; 409 si le nom est déjà pris dans la catégorie */
export async function createTag(payload: CreateTagPayload): Promise<TagDto> {
  const { data } = await api.post<TagDto>('/api/admin/tags', payload);
  return data;
}
