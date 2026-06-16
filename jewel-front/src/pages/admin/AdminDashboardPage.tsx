import { Link } from 'react-router-dom';
import { PageHeader } from '../../components/ui/PageHeader';
import { Card } from '../../components/ui/Card';
import { GemIcon } from '../../components/icons-admin';

/** Accueil de la zone admin (page principale). */
export function AdminDashboardPage() {
  return (
    <div className="mx-auto w-full max-w-[760px]">
      <PageHeader
        title="Tableau de bord"
        subtitle="Gérez le catalogue et les utilisateurs de la boutique"
      />

      <div className="grid gap-4 sm:grid-cols-2">
        <Link to="/admin/products/new" className="block">
          <Card className="h-full transition-shadow hover:shadow-btn">
            <div className="flex items-center gap-3 text-storm">
              <span className="flex h-10 w-10 items-center justify-center rounded-xl bg-blush text-fuchsia">
                <GemIcon width={20} height={20} />
              </span>
              <div>
                <p className="font-body text-sm font-semibold">Créer un produit</p>
                <p className="font-body text-[13px] text-slate">Breloque ou chaîne</p>
              </div>
            </div>
          </Card>
        </Link>
      </div>
    </div>
  );
}
