import { observer } from 'mobx-react-lite';
import { useAuthStore } from '../../stores/context';
import { LogoutIcon } from '../../components/icons-admin';
import { adminNavItems } from './navItems';
import { SidebarLink } from './SidebarLink';

interface AdminSidebarProps {
  /** Appelé après navigation (pour refermer le tiroir en mobile). */
  onNavigate?: () => void;
}

export const AdminSidebar = observer(function AdminSidebar({ onNavigate }: AdminSidebarProps) {
  const auth = useAuthStore();

  return (
    <aside className="flex h-full w-[260px] flex-col bg-[linear-gradient(180deg,#384959_0%,#46607a_100%)] px-4 py-6 text-white">
      {/* Marque */}
      <div className="px-3.5 pb-6">
        <p className="font-display text-[26px] font-light leading-none tracking-[0.04em]">
          Jewellery Shop
        </p>
        <p className="mt-1 font-body text-[11px] uppercase tracking-[0.2em] text-poudre">
          Administration
        </p>
      </div>

      {/* Navigation */}
      <nav className="flex flex-1 flex-col gap-1">
        {adminNavItems.map((item) => (
          <SidebarLink
            key={item.to}
            to={item.to}
            icon={item.icon}
            label={item.label}
            end={item.end}
            onNavigate={onNavigate}
          />
        ))}
      </nav>

      {/* Pied : utilisateur + déconnexion */}
      <div className="mt-4 border-t border-white/10 pt-4">
        <p className="px-3.5 font-body text-[11px] text-mist/70">Connecté</p>
        <p className="mb-3 px-3.5 font-body text-sm font-medium text-white">
          Admin #{auth.userId ?? '—'}
        </p>
        <button
          onClick={() => auth.logout()}
          className="flex w-full items-center gap-3 rounded-xl px-3.5 py-2.5 font-body text-sm font-medium text-mist/80 transition-all hover:bg-white/10 hover:text-white"
        >
          <LogoutIcon />
          Se déconnecter
        </button>
      </div>
    </aside>
  );
});
