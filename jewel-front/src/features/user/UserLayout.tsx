import { useEffect } from 'react';
import { observer } from 'mobx-react-lite';
import { Link, Outlet } from 'react-router-dom';
import { useAuthStore, useFavoritesStore } from '../../stores/context';
import { GemIcon, ChainIcon, HeartIcon, LogoutIcon } from '../../components/icons-admin';
import { NavbarLink } from './NavbarLink';

/** Gabarit de la zone utilisateur : barre de navigation horizontale + contenu. */
export const UserLayout = observer(function UserLayout() {
  const auth = useAuthStore();
  const favorites = useFavoritesStore();

  // Charge les favoris pour la session courante.
  useEffect(() => {
    favorites.reset();
    void favorites.load();
  }, [favorites, auth.userId]);

  const handleLogout = () => {
    favorites.reset();
    auth.logout();
  };

  return (
    <div className="flex h-screen w-screen flex-col overflow-hidden bg-creme">
      <header className="flex items-center gap-4 border-b border-lin bg-white px-5 py-3">
        <Link to="/charms" className="font-display text-[22px] font-light tracking-[0.04em] text-storm">
          Jewellery Shop
        </Link>

        <nav className="ml-4 flex items-center gap-1">
          <NavbarLink to="/charms" label="Breloques" icon={<GemIcon width={16} height={16} />} />
          <NavbarLink to="/chains" label="Chaînes" icon={<ChainIcon width={16} height={16} />} />
          <NavbarLink to="/favorites" label="Favoris" icon={<HeartIcon width={16} height={16} />} />
        </nav>

        <div className="ml-auto flex items-center gap-3">
          {auth.isAdmin && (
            <Link
              to="/admin"
              className="font-body text-[13px] font-medium text-slate transition-colors hover:text-fuchsia"
            >
              Admin
            </Link>
          )}
          <button
            onClick={handleLogout}
            className="inline-flex items-center gap-1.5 font-body text-[13px] font-medium text-slate transition-colors hover:text-fuchsia"
          >
            <LogoutIcon width={16} height={16} />
            Déconnexion
          </button>
        </div>
      </header>

      <main className="flex-1 overflow-auto p-6 md:p-10">
        <Outlet />
      </main>
    </div>
  );
});
