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

export const INVITATION_STATUSES = ['Pending', 'Expired', 'Revoked', 'Accepted'] as const;
export type InvitationStatus = (typeof INVITATION_STATUSES)[number];

export interface Invitation {
  id: number;
  email: string;
  /** Statut effectif : une invitation en attente dont l'échéance est passée est renvoyée en `Expired`. */
  status: InvitationStatus;
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

/** GET /api/admin/invitations → PagedResult<Invitation> */
export async function getInvitations(
  status: InvitationStatus,
  page: number,
  pageSize: number
): Promise<PagedResult<Invitation>> {
  const { data } = await api.get<PagedResult<Invitation>>('/api/admin/invitations', {
    params: { status, page, pageSize },
  });
  return data;
}

/** POST /api/admin/invitations → 201 Invitation */
export async function createInvitation(email: string): Promise<Invitation> {
  const { data } = await api.post<Invitation>('/api/admin/invitations', { email });
  return data;
}

/** POST /api/admin/invitations/{id}/resend → 200 Invitation */
export async function resendInvitation(id: number): Promise<Invitation> {
  const { data } = await api.post<Invitation>(`/api/admin/invitations/${id}/resend`);
  return data;
}

/** POST /api/admin/invitations/{id}/revoke → 204 */
export async function revokeInvitation(id: number): Promise<void> {
  await api.post(`/api/admin/invitations/${id}/revoke`);
}
