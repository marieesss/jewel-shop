import type { ReactNode } from 'react';
import { NavLink } from 'react-router-dom';
import { cn } from '../../lib/cn';

interface NavbarLinkProps {
  to: string;
  icon?: ReactNode;
  label: string;
}

/** Lien de navigation horizontale, état actif géré par React Router. */
export function NavbarLink({ to, icon, label }: NavbarLinkProps) {
  return (
    <NavLink
      to={to}
      className={({ isActive }) =>
        cn(
          'inline-flex items-center gap-1.5 rounded-lg px-3 py-2 font-body text-sm font-medium transition-colors',
          isActive ? 'bg-blush text-fuchsia' : 'text-slate hover:text-storm'
        )
      }
    >
      {icon}
      {label}
    </NavLink>
  );
}
