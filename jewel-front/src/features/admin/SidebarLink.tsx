import type { ReactNode } from 'react';
import { NavLink } from 'react-router-dom';
import { cn } from '../../lib/cn';

interface SidebarLinkProps {
  to: string;
  icon: ReactNode;
  label: string;
  end?: boolean;
  onNavigate?: () => void;
}

/** Élément de navigation latérale, avec état actif géré par React Router. */
export function SidebarLink({ to, icon, label, end, onNavigate }: SidebarLinkProps) {
  return (
    <NavLink
      to={to}
      end={end}
      onClick={onNavigate}
      className={({ isActive }) =>
        cn(
          'flex items-center gap-3 rounded-xl px-3.5 py-2.5 font-body text-sm font-medium transition-all',
          isActive
            ? 'bg-white/15 text-white'
            : 'text-mist/80 hover:bg-white/10 hover:text-white'
        )
      }
    >
      <span className="flex h-[18px] w-[18px] items-center justify-center">{icon}</span>
      {label}
    </NavLink>
  );
}
