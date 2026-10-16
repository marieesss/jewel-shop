import { observer } from 'mobx-react-lite';
import { Navigate, Outlet, useLocation, type Location } from 'react-router-dom';
import { useAuthStore } from '../stores/context';
import { loginPathFor } from '../lib/returnUrl';
import type { LoginLocationState } from '../lib/sessionExpired';
import { Spinner } from '../components/ui/Spinner';

/** Redirection vers /login avec la page demandée en returnUrl, en signalant une session expirée. */
function redirectToLogin(location: Location, sessionExpired: boolean) {
  const state: LoginLocationState | undefined = sessionExpired
    ? { sessionExpired: true }
    : undefined;
  return <Navigate to={loginPathFor(location)} state={state} replace />;
}

/** Exige une session authentifiée ; sinon redirige vers /login avec la page demandée en returnUrl. */
export const ProtectedRoute = observer(function ProtectedRoute() {
  const auth = useAuthStore();
  const location = useLocation();

  if (auth.initializing) {
    return <Spinner className="h-screen" label="Chargement…" />;
  }
  if (!auth.isAuthenticated) {
    return redirectToLogin(location, auth.sessionExpired);
  }
  return <Outlet />;
});

/** Exige le rôle Admin ; non connecté → /login, connecté non-admin → accueil. */
export const AdminRoute = observer(function AdminRoute() {
  const auth = useAuthStore();
  const location = useLocation();

  if (auth.initializing) {
    return <Spinner className="h-screen" label="Chargement…" />;
  }
  if (!auth.isAuthenticated) {
    return redirectToLogin(location, auth.sessionExpired);
  }
  if (!auth.isAdmin) {
    return <Navigate to="/" replace />;
  }
  return <Outlet />;
});
