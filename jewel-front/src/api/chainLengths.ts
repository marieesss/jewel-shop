import { api } from './client';

export interface ChainLength {
  id: number;
  lengthCm: number;
  label: string;
}

/** Champs saisis à la création ou à la modification d'une longueur. */
export interface ChainLengthInput {
  lengthCm: number;
  label: string;
}

/** GET /api/admin/chain-lengths → ChainLength[], de la plus courte à la plus longue */
export async function getAdminChainLengths(): Promise<ChainLength[]> {
  const { data } = await api.get<ChainLength[]>('/api/admin/chain-lengths');
  return data;
}

/** POST /api/admin/chain-lengths → 201 ChainLength */
export async function createChainLength(input: ChainLengthInput): Promise<ChainLength> {
  const { data } = await api.post<ChainLength>('/api/admin/chain-lengths', input);
  return data;
}

/** PUT /api/admin/chain-lengths/{id} → 200 ChainLength */
export async function updateChainLength(id: number, input: ChainLengthInput): Promise<ChainLength> {
  const { data } = await api.put<ChainLength>(`/api/admin/chain-lengths/${id}`, input);
  return data;
}

/** DELETE /api/admin/chain-lengths/{id} → 204 */
export async function deleteChainLength(id: number): Promise<void> {
  await api.delete(`/api/admin/chain-lengths/${id}`);
}
