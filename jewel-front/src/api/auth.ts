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
