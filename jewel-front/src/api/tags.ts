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

/** Tag dont la catégorie est l'une des valeurs connues de `TAG_CATEGORIES`. */
export interface Tag extends TagDto {
  category: TagCategory;
}

/** Tag de la liste admin, avec le nombre de chaînes et de breloques qui le portent. */
export interface AdminTag extends Tag {
  chainCount: number;
  charmCount: number;
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

/** GET /api/admin/tags → AdminTag[], triés par catégorie puis par nom */
export async function getAdminTags(): Promise<AdminTag[]> {
  const { data } = await api.get<AdminTag[]>('/api/admin/tags');
  return data;
}

/** POST /api/admin/tags (Admin) → 201 TagDto ; 409 si le nom est déjà pris dans la catégorie */
export async function createTag(payload: CreateTagPayload): Promise<TagDto> {
  const { data } = await api.post<TagDto>('/api/admin/tags', payload);
  return data;
}

/** PUT /api/admin/tags/{id} → 200 Tag (la catégorie ne change pas) */
export async function renameTag(id: number, name: string): Promise<Tag> {
  const { data } = await api.put<Tag>(`/api/admin/tags/${id}`, { name });
  return data;
}

/** DELETE /api/admin/tags/{id} → 204 */
export async function deleteTag(id: number): Promise<void> {
  await api.delete(`/api/admin/tags/${id}`);
}

/** POST /api/admin/tags/{id}/merge → 200 Tag cible ; le tag {id} est supprimé */
export async function mergeTag(id: number, targetTagId: number): Promise<Tag> {
  const { data } = await api.post<Tag>(`/api/admin/tags/${id}/merge`, { targetTagId });
  return data;
}
