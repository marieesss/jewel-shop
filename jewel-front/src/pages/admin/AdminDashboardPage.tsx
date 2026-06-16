import { PageHeader } from '../../components/ui/PageHeader';
import { DashboardTile } from '../../features/admin/DashboardTile';
import { GemIcon, ChainIcon } from '../../components/icons-admin';

/** Accueil de la zone admin (page principale). */
export function AdminDashboardPage() {
  return (
    <div className="mx-auto w-full max-w-[760px]">
      <PageHeader
        title="Tableau de bord"
        subtitle="Gérez le catalogue et les utilisateurs de la boutique"
      />

      <div className="grid gap-4 sm:grid-cols-2">
        <DashboardTile
          to="/admin/products/new"
          icon={<GemIcon width={20} height={20} />}
          title="Créer un produit"
          description="Breloque ou chaîne"
        />
        <DashboardTile
          to="/admin/products/charms"
          icon={<GemIcon width={20} height={20} />}
          title="Breloques"
          description="Lister les breloques"
        />
        <DashboardTile
          to="/admin/products/chains"
          icon={<ChainIcon width={20} height={20} />}
          title="Chaînes"
          description="Lister les chaînes"
        />
      </div>
    </div>
  );
}
