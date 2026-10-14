import { observer } from 'mobx-react-lite';
import { Navigate, Outlet, useLocation } from 'react-router-dom';
import { useAuthStore } from '../stores/context';

/** Exige une session authentifiée ; sinon redirige vers /login. */
export const ProtectedRoute = observer(function ProtectedRoute() {
  const auth = useAuthStore();
  const location = useLocation();

  if (!auth.isAuthenticated) {
    return <Navigate to="/login" replace state={{ from: location }} />;
  }
  return <Outlet />;
});

/** Exige le rôle Admin ; non connecté → /login, connecté non-admin → accueil. */
export const AdminRoute = observer(function AdminRoute() {
  const auth = useAuthStore();
  const location = useLocation();

  if (!auth.isAuthenticated) {
    return <Navigate to="/login" replace state={{ from: location }} />;
  }
  if (!auth.isAdmin) {
    return <Navigate to="/" replace />;
  }
  return <Outlet />;
});
