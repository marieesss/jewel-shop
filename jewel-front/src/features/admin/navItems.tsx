import type { ReactNode } from 'react';
import { DashboardIcon, GemIcon, ChainIcon } from '../../components/icons-admin';

export interface AdminNavItem {
  to: string;
  label: string;
  icon: ReactNode;
  /** Correspondance exacte (pour l'item racine du tableau de bord). */
  end?: boolean;
}

/**
 * Source unique de la navigation admin : ajouter une page = ajouter une entrée.
 * La sidebar se construit à partir de cette liste.
 */
export const adminNavItems: AdminNavItem[] = [
  { to: '/admin', label: 'Tableau de bord', icon: <DashboardIcon />, end: true },
  { to: '/admin/products/new', label: 'Créer un produit', icon: <GemIcon /> },
  { to: '/admin/products/charms', label: 'Breloques', icon: <GemIcon /> },
  { to: '/admin/products/chains', label: 'Chaînes', icon: <ChainIcon /> },
];
