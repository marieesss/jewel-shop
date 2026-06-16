import type { ReactNode } from 'react';
import { Link } from 'react-router-dom';
import { Card } from '../../components/ui/Card';

interface DashboardTileProps {
  to: string;
  icon: ReactNode;
  title: string;
  description: string;
}

/** Tuile d'accès rapide du tableau de bord admin. */
export function DashboardTile({ to, icon, title, description }: DashboardTileProps) {
  return (
    <Link to={to} className="block">
      <Card className="h-full transition-all hover:-translate-y-0.5 hover:shadow-btn">
        <div className="flex items-center gap-3 text-storm">
          <span className="flex h-10 w-10 shrink-0 items-center justify-center rounded-xl bg-blush text-fuchsia">
            {icon}
          </span>
          <div>
            <p className="font-body text-sm font-semibold">{title}</p>
            <p className="font-body text-[13px] text-slate">{description}</p>
          </div>
        </div>
      </Card>
    </Link>
  );
}
