import { useState } from 'react';
import { Outlet } from 'react-router-dom';
import { cn } from '../../lib/cn';
import { MenuIcon } from '../../components/icons-admin';
import { AdminSidebar } from './AdminSidebar';

/**
 * Gabarit de la zone admin : sidebar persistante (desktop) ou tiroir (mobile)
 * + zone de contenu rendant la route enfant via <Outlet />.
 */
export function AdminLayout() {
  const [drawerOpen, setDrawerOpen] = useState(false);

  return (
    <div className="flex h-screen w-screen overflow-hidden bg-creme">
      {/* Sidebar desktop */}
      <div className="hidden md:flex">
        <AdminSidebar />
      </div>

      {/* Tiroir mobile */}
      <div
        className={cn(
          'fixed inset-0 z-40 md:hidden',
          drawerOpen ? 'pointer-events-auto' : 'pointer-events-none'
        )}
      >
        <div
          className={cn(
            'absolute inset-0 bg-storm/40 transition-opacity',
            drawerOpen ? 'opacity-100' : 'opacity-0'
          )}
          onClick={() => setDrawerOpen(false)}
        />
        <div
          className={cn(
            'absolute left-0 top-0 h-full transition-transform duration-300',
            drawerOpen ? 'translate-x-0' : '-translate-x-full'
          )}
        >
          <AdminSidebar onNavigate={() => setDrawerOpen(false)} />
        </div>
      </div>

      {/* Contenu */}
      <div className="flex flex-1 flex-col overflow-hidden">
        {/* Barre mobile */}
        <div className="flex items-center gap-3 border-b border-lin bg-white px-4 py-3 md:hidden">
          <button
            onClick={() => setDrawerOpen(true)}
            aria-label="Ouvrir le menu"
            className="text-storm"
          >
            <MenuIcon />
          </button>
          <span className="font-display text-lg font-light text-storm">Jewellery Shop</span>
        </div>

        <main className="flex-1 overflow-auto p-6 md:p-10">
          <Outlet />
        </main>
      </div>
    </div>
  );
}
