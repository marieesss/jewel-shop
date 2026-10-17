import { api } from './client';

export interface AuthResponse {
  token: string;
  userId: number;
  role: string;
}

export interface RegisterPayload {
  name: string;
  surname: string;
  email: string;
  password: string;
  birthday?: string | null;
  invitationToken?: string;
}

export interface InvitationDetails {
  email: string;
  expiresAt: string;
}

export interface LoginPayload {
  email: string;
  password: string;
}

/** POST /api/auth/login → 200 { token, userId, role } */
export async function login(payload: LoginPayload): Promise<AuthResponse> {
  const { data } = await api.post<AuthResponse>('/api/auth/login', payload);
  return data;
}

/** POST /api/auth/register → 201 { token, userId, role } */
export async function register(payload: RegisterPayload): Promise<AuthResponse> {
  const { data } = await api.post<AuthResponse>('/api/auth/register', payload);
  return data;
}

/** POST /api/auth/refresh → 200 { token, userId, role }, avec rotation du cookie refresh_token */
export async function refresh(): Promise<AuthResponse> {
  const { data } = await api.post<AuthResponse>('/api/auth/refresh');
  return data;
}

/** POST /api/auth/logout → 204, révoque le refresh token et supprime le cookie */
export async function logout(): Promise<void> {
  await api.post('/api/auth/logout');
}

/** POST /api/auth/forgot-password → 202, que l'email soit inscrit ou non */
export async function forgotPassword(email: string): Promise<void> {
  await api.post('/api/auth/forgot-password', { email });
}

/** POST /api/auth/reset-password → 204, 400 si le token est invalide, utilisé ou expiré */
export async function resetPassword(token: string, password: string): Promise<void> {
  await api.post('/api/auth/reset-password', { token, password });
}

/** GET /api/auth/invitations/{token} → { email, expiresAt } */
export async function getInvitation(token: string): Promise<InvitationDetails> {
  const { data } = await api.get<InvitationDetails>(
    `/api/auth/invitations/${encodeURIComponent(token)}`
  );
  return data;
}
