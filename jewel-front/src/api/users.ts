import { api } from './client';

export interface AddressDto {
  line1: string;
  line2: string | null;
  postalCode: string;
  city: string;
}

export interface ProfileDto {
  id: number;
  name: string;
  surname: string;
  email: string;
  birthday: string | null;
  phone: string | null;
  /** Null tant qu'aucune adresse n'est enregistrée. */
  address: AddressDto | null;
}

/** Adresse de livraison (France uniquement) et téléphone envoyés à l'enregistrement. */
export interface AddressInput {
  line1: string;
  line2: string | null;
  postalCode: string;
  city: string;
  phone: string;
}

/** GET /api/users/me → profil de l'utilisateur connecté */
export async function getMyProfile(): Promise<ProfileDto> {
  const { data } = await api.get<ProfileDto>('/api/users/me');
  return data;
}

/** PUT /api/users/me/address → 200 ProfileDto */
export async function updateMyAddress(input: AddressInput): Promise<ProfileDto> {
  const { data } = await api.put<ProfileDto>('/api/users/me/address', input);
  return data;
}
