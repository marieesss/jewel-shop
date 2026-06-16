import type { FormEvent, ReactNode } from 'react';
import { Card } from '../../components/ui/Card';
import { Alert } from '../../components/ui/Alert';
import { Button } from '../../components/ui/Button';
import { ProductCommonFields } from './ProductCommonFields';
import type { FieldErrors, ProductCommonValues } from './productForm';

interface Props {
  onSubmit: (e: FormEvent) => void;
  common: ProductCommonValues;
  onCommonChange: (patch: Partial<ProductCommonValues>) => void;
  commonErrors: FieldErrors<keyof ProductCommonValues>;
  /** Champs spécifiques au type de produit (stock, longueur…). */
  children: ReactNode;
  submitLabel: string;
  loading: boolean;
  error: string | null;
  success: string | null;
}

/**
 * Ossature commune de création de produit : carte + champs partagés + champs
 * spécifiques (slot) + retours d'état + bouton. Réutilisé par chaque type.
 */
export function ProductFormShell({
  onSubmit,
  common,
  onCommonChange,
  commonErrors,
  children,
  submitLabel,
  loading,
  error,
  success,
}: Props) {
  return (
    <form onSubmit={onSubmit} noValidate>
      <Card>
        <div className="flex flex-col gap-[18px]">
          <ProductCommonFields values={common} onChange={onCommonChange} errors={commonErrors} />
          {children}
        </div>

        {error && <Alert className="mt-5">{error}</Alert>}
        {success && (
          <Alert variant="success" className="mt-5">
            {success}
          </Alert>
        )}

        <Button type="submit" fullWidth loading={loading} className="mt-6">
          {loading ? 'Enregistrement…' : submitLabel}
        </Button>
      </Card>
    </form>
  );
}
