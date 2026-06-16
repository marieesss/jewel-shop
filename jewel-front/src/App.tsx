import { BrowserRouter, Routes, Route, Navigate } from 'react-router-dom';
import { StoreProvider, rootStore } from './stores/context';
import { RequireAuth, RequireAdmin } from './routes/guards';
import { AuthPage } from './pages/AuthPage';
import { HomePage } from './pages/HomePage';
import { AdminLayout } from './features/admin/AdminLayout';
import { AdminDashboardPage } from './pages/admin/AdminDashboardPage';
import { ProductCreatePage } from './pages/admin/ProductCreatePage';

export default function App() {
  return (
    <StoreProvider value={rootStore}>
      <BrowserRouter>
        <Routes>
          {/* Public */}
          <Route path="/login" element={<AuthPage />} />

          {/* Connecté */}
          <Route element={<RequireAuth />}>
            <Route path="/" element={<HomePage />} />
          </Route>

          {/* Admin uniquement */}
          <Route element={<RequireAdmin />}>
            <Route path="/admin" element={<AdminLayout />}>
              <Route index element={<AdminDashboardPage />} />
              <Route path="products/new" element={<ProductCreatePage />} />
            </Route>
          </Route>

          {/* Repli */}
          <Route path="*" element={<Navigate to="/" replace />} />
        </Routes>
      </BrowserRouter>
    </StoreProvider>
  );
}
