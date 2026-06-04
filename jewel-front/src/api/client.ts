import axios from 'axios';

const baseURL = import.meta.env.VITE_API_URL ?? 'http://localhost:5030';

export const api = axios.create({
  baseURL,
  headers: { 'Content-Type': 'application/json' },
});

const TOKEN_KEY = import.meta.env.VITE_JWT_SECRET ?? (() => { throw new Error('JWT_SECRET is not defined in environment variables'); })();

/** Injecte le Bearer token sur chaque requête s'il est présent. */
api.interceptors.request.use((config) => {
  const token = localStorage.getItem(TOKEN_KEY);
  if (token) {
    config.headers.Authorization = `Bearer ${token}`;
  }
  return config;
});

export { TOKEN_KEY };
