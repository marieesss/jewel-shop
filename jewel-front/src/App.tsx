import { BrowserRouter, Routes, Route, Navigate } from 'react-router-dom';
import { StoreProvider, rootStore } from './stores/context';
import { RequireAuth, RequireAdmin } from './routes/guards';
import { AuthPage } from './pages/AuthPage';
import { UserLayout } from './features/user/UserLayout';
import { UserProductListPage } from './pages/user/UserProductListPage';
import { UserProductDetailPage } from './pages/user/UserProductDetailPage';
import { FavoritesPage } from './pages/user/FavoritesPage';
import { AdminLayout } from './features/admin/AdminLayout';
import { AdminDashboardPage } from './pages/admin/AdminDashboardPage';
import { ProductCreatePage } from './pages/admin/ProductCreatePage';
import { ProductListPage } from './pages/admin/ProductListPage';
import { ProductDetailPage } from './pages/admin/ProductDetailPage';
import { ProductEditPage } from './pages/admin/ProductEditPage';

export default function App() {
  return (
    <StoreProvider value={rootStore}>
      <BrowserRouter>
        <Routes>
          {/* Public */}
          <Route path="/login" element={<AuthPage />} />

          {/* Espace utilisateur connecté (navbar horizontale) */}
          <Route element={<RequireAuth />}>
            <Route element={<UserLayout />}>
              <Route index element={<Navigate to="/charms" replace />} />
              <Route path="/charms" element={<UserProductListPage kind="charm" />} />
              <Route path="/charms/:id" element={<UserProductDetailPage kind="charm" />} />
              <Route path="/chains" element={<UserProductListPage kind="chain" />} />
              <Route path="/chains/:id" element={<UserProductDetailPage kind="chain" />} />
              <Route path="/favorites" element={<FavoritesPage />} />
            </Route>
          </Route>

          {/* Admin uniquement */}
          <Route element={<RequireAdmin />}>
            <Route path="/admin" element={<AdminLayout />}>
              <Route index element={<AdminDashboardPage />} />
              <Route path="products/new" element={<ProductCreatePage />} />
              <Route path="products/charms" element={<ProductListPage kind="charm" />} />
              <Route path="products/charms/:id" element={<ProductDetailPage kind="charm" />} />
              <Route path="products/charms/:id/edit" element={<ProductEditPage kind="charm" />} />
              <Route path="products/chains" element={<ProductListPage kind="chain" />} />
              <Route path="products/chains/:id" element={<ProductDetailPage kind="chain" />} />
              <Route path="products/chains/:id/edit" element={<ProductEditPage kind="chain" />} />
            </Route>
          </Route>

          {/* Repli */}
          <Route path="*" element={<Navigate to="/" replace />} />
        </Routes>
      </BrowserRouter>
    </StoreProvider>
  );
}
