import { useCallback, useEffect, useState } from 'react';
import { extractApiError } from '../api/errors';

interface AsyncState<T> {
  data: T | null;
  loading: boolean;
  error: string | null;
  reload: () => void;
}

/**
 * Exécute une fonction asynchrone au montage et à chaque changement de `deps`,
 * en exposant data/loading/error et un `reload`. Annule proprement si le
 * composant se démonte (évite les set d'état tardifs).
 */
export function useAsync<T>(fn: () => Promise<T>, deps: unknown[]): AsyncState<T> {
  const [data, setData] = useState<T | null>(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [tick, setTick] = useState(0);

  const reload = useCallback(() => setTick((t) => t + 1), []);

  useEffect(() => {
    let active = true;
    setLoading(true);
    setError(null);

    fn()
      .then((result) => {
        if (active) setData(result);
      })
      .catch((err) => {
        if (active) setError(extractApiError(err));
      })
      .finally(() => {
        if (active) setLoading(false);
      });

    return () => {
      active = false;
    };
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [...deps, tick]);

  return { data, loading, error, reload };
}
