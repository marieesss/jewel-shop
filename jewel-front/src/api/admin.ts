import { api } from './client';
import type { PagedResult } from './products';

export interface AdminUser {
  id: number;
  name: string;
  surname: string;
  email: string;
  /** Date seule, au format `yyyy-MM-dd`. */
  birthday: string | null;
  role: string;
  /** Date UTC, `null` pour les comptes créés avant l'ajout de la date d'inscription. */
  createdAt: string | null;
}

export interface Invitation {
  id: number;
  email: string;
  status: string;
  expiresAt: string;
  createdAt: string;
  invitedBy: string | null;
}

/** GET /api/admin/users → PagedResult<AdminUser> */
export async function getUsers(page: number, pageSize: number): Promise<PagedResult<AdminUser>> {
  const { data } = await api.get<PagedResult<AdminUser>>('/api/admin/users', {
    params: { page, pageSize },
  });
  return data;
}

/** POST /api/admin/invitations → 201 Invitation */
export async function createInvitation(email: string): Promise<Invitation> {
  const { data } = await api.post<Invitation>('/api/admin/invitations', { email });
  return data;
}
