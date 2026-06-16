import { Link, useNavigate, useParams } from 'react-router-dom';
import { PageHeader } from '../../components/ui/PageHeader';
import { Spinner } from '../../components/ui/Spinner';
import { Alert } from '../../components/ui/Alert';
import { useAsync } from '../../hooks/useAsync';
import { PRODUCT_CONFIG } from '../../features/products/productConfig';
import { CharmForm } from '../../features/products/CharmForm';
import { ChainForm } from '../../features/products/ChainForm';
import type { CharmDto, ChainDto } from '../../api/products';
import type { ProductKind } from '../../stores/ProductAdminStore';

interface ProductEditPageProps {
  kind: ProductKind;
}

/** Page d'édition d'un produit : réutilise le formulaire en mode « édition ». */
export function ProductEditPage({ kind }: ProductEditPageProps) {
  const config = PRODUCT_CONFIG[kind];
  const { id } = useParams();
  const productId = Number(id);
  const navigate = useNavigate();

  const { data, loading, error } = useAsync(
    () => config.fetchById(productId),
    [kind, productId]
  );

  const goToDetail = () => navigate(config.detailPath(productId));

  return (
    <div className="mx-auto w-full max-w-[560px]">
      <Link
        to={config.detailPath(productId)}
        className="mb-5 inline-flex items-center gap-1 font-body text-[13px] font-medium text-slate transition-colors hover:text-fuchsia"
      >
        ← Retour au détail
      </Link>

      <PageHeader title={`Modifier — ${config.singular.toLowerCase()}`} />

      {loading && <Spinner label="Chargement…" />}
      {!loading && (error || Number.isNaN(productId)) && (
        <Alert>{error ?? 'Identifiant de produit invalide.'}</Alert>
      )}

      {!loading && !error && data && (
        kind === 'charm' ? (
          <CharmForm product={data as CharmDto} onSuccess={goToDetail} />
        ) : (
          <ChainForm product={data as ChainDto} onSuccess={goToDetail} />
        )
      )}
    </div>
  );
}
