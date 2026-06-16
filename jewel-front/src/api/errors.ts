import { AxiosError } from 'axios';

/** Forme du corps d'erreur renvoyé par l'API (ExceptionHandlingMiddleware). */
interface ApiErrorBody {
  status?: number;
  title?: string;
  message?: string;
  errors?: Record<string, string[]> | null;
}

/**
 * Transforme une erreur Axios en message lisible pour l'utilisateur, en
 * privilégiant les erreurs de validation détaillées de l'API.
 */
export function extractApiError(err: unknown, fallback = 'Une erreur est survenue. Veuillez réessayer.'): string {
  if (err instanceof AxiosError) {
    const data = err.response?.data as ApiErrorBody | undefined;

    if (data?.errors) {
      const first = Object.values(data.errors).flat()[0];
      if (first) return first;
    }
    if (data?.message) return data.message;
    if (data?.title) return data.title;

    if (!err.response) return 'Impossible de joindre le serveur.';
    if (err.response.status === 401) return "Vous n'êtes pas autorisé à effectuer cette action.";
    if (err.response.status === 403) return "Accès réservé aux administrateurs.";
  }
  return fallback;
}
