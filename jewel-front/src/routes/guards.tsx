import { observer } from 'mobx-react-lite';
import { Navigate, Outlet, useLocation } from 'react-router-dom';
import { useAuthStore } from '../stores/context';
import { loginPathFor } from '../lib/returnUrl';

/** Exige une session authentifiée ; sinon redirige vers /login avec la page demandée en returnUrl. */
export const ProtectedRoute = observer(function ProtectedRoute() {
  const auth = useAuthStore();
  const location = useLocation();

  if (!auth.isAuthenticated) {
    return <Navigate to={loginPathFor(location)} replace />;
  }
  return <Outlet />;
});

/** Exige le rôle Admin ; non connecté → /login, connecté non-admin → accueil. */
export const AdminRoute = observer(function AdminRoute() {
  const auth = useAuthStore();
  const location = useLocation();

  if (!auth.isAuthenticated) {
    return <Navigate to={loginPathFor(location)} replace />;
  }
  if (!auth.isAdmin) {
    return <Navigate to="/" replace />;
  }
  return <Outlet />;
});
