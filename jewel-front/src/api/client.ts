import axios from 'axios';

const baseURL = import.meta.env.VITE_API_URL ?? 'http://localhost:5030';

/** Client HTTP unique. L'access token et le refresh sur 401 sont branchés par `AuthStore`. */
export const api = axios.create({
  baseURL,
  headers: { 'Content-Type': 'application/json' },
  // Envoie le cookie httpOnly refresh_token, y compris en dev où l'API est sur un autre port.
  withCredentials: true,
});
