import { api } from './client';

/** Catégories de tag, dans l'ordre de tri de l'API. */
export const TAG_CATEGORIES = ['Theme', 'Color', 'Material', 'Occasion'] as const;

export type TagCategory = (typeof TAG_CATEGORIES)[number];

export interface Tag {
  id: number;
  name: string;
  slug: string;
  category: TagCategory;
}

/** Tag de la liste admin, avec le nombre de chaînes et de breloques qui le portent. */
export interface AdminTag extends Tag {
  chainCount: number;
  charmCount: number;
}

export interface CreateTagInput {
  name: string;
  category: TagCategory;
}

/** GET /api/admin/tags → AdminTag[], triés par catégorie puis par nom */
export async function getAdminTags(): Promise<AdminTag[]> {
  const { data } = await api.get<AdminTag[]>('/api/admin/tags');
  return data;
}

/** POST /api/admin/tags → 201 Tag */
export async function createTag(input: CreateTagInput): Promise<Tag> {
  const { data } = await api.post<Tag>('/api/admin/tags', input);
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
