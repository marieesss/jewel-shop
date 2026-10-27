import { BrowserRouter, Routes, Route, Navigate } from 'react-router-dom';
import { StoreProvider, rootStore } from './stores/context';
import { ProtectedRoute, AdminRoute } from './routes/guards';
import { AuthPage } from './pages/AuthPage';
import { ForgotPasswordPage } from './pages/ForgotPasswordPage';
import { ResetPasswordPage } from './pages/ResetPasswordPage';
import { RegisterPage } from './pages/RegisterPage';
import { UserLayout } from './features/user/UserLayout';
import { UserProductListPage } from './pages/user/UserProductListPage';
import { UserProductDetailPage } from './pages/user/UserProductDetailPage';
import { FavoritesPage } from './pages/user/FavoritesPage';
import { ProfilePage } from './pages/user/ProfilePage';
import { AdminLayout } from './features/admin/AdminLayout';
import { AdminDashboardPage } from './pages/admin/AdminDashboardPage';
import { ProductCreatePage } from './pages/admin/ProductCreatePage';
import { ProductListPage } from './pages/admin/ProductListPage';
import { ProductDetailPage } from './pages/admin/ProductDetailPage';
import { ProductEditPage } from './pages/admin/ProductEditPage';
import { AdminUsersPage } from './pages/admin/AdminUsersPage';
import { AdminChainLengthsPage } from './pages/admin/AdminChainLengthsPage';

export default function App() {
  return (
    <StoreProvider value={rootStore}>
      <BrowserRouter>
        <Routes>
          {/* Public */}
          <Route path="/login" element={<AuthPage />} />
          <Route path="/forgot-password" element={<ForgotPasswordPage />} />
          <Route path="/reset-password" element={<ResetPasswordPage />} />
          <Route path="/register" element={<RegisterPage />} />

          {/* Espace utilisateur connecté (navbar horizontale) */}
          <Route element={<ProtectedRoute />}>
            <Route element={<UserLayout />}>
              <Route index element={<Navigate to="/charms" replace />} />
              <Route path="/charms" element={<UserProductListPage kind="charm" />} />
              <Route path="/charms/:id" element={<UserProductDetailPage kind="charm" />} />
              <Route path="/chains" element={<UserProductListPage kind="chain" />} />
              <Route path="/chains/:id" element={<UserProductDetailPage kind="chain" />} />
              <Route path="/favorites" element={<FavoritesPage />} />
              <Route path="/profile" element={<ProfilePage />} />
            </Route>
          </Route>

          {/* Admin uniquement */}
          <Route element={<AdminRoute />}>
            <Route path="/admin" element={<AdminLayout />}>
              <Route index element={<AdminDashboardPage />} />
              <Route path="products/new" element={<ProductCreatePage />} />
              <Route path="products/charms" element={<ProductListPage kind="charm" />} />
              <Route path="products/charms/:id" element={<ProductDetailPage kind="charm" />} />
              <Route path="products/charms/:id/edit" element={<ProductEditPage kind="charm" />} />
              <Route path="products/chains" element={<ProductListPage kind="chain" />} />
              <Route path="products/chains/:id" element={<ProductDetailPage kind="chain" />} />
              <Route path="products/chains/:id/edit" element={<ProductEditPage kind="chain" />} />
              <Route path="chain-lengths" element={<AdminChainLengthsPage />} />
              <Route path="users" element={<AdminUsersPage />} />
            </Route>
          </Route>

          {/* Repli */}
          <Route path="*" element={<Navigate to="/" replace />} />
        </Routes>
      </BrowserRouter>
    </StoreProvider>
  );
}
