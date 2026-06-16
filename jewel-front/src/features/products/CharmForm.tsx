import { useEffect, useState, type FormEvent } from 'react';
import { observer } from 'mobx-react-lite';
import { useProductAdminStore } from '../../stores/context';
import { TextField } from '../../components/ui/TextField';
import { ImageUpload } from '../../components/ui/ImageUpload';
import { ProductFormShell } from './ProductFormShell';
import {
  commonFromProduct,
  emptyProductCommon,
  toNumber,
  validateCommon,
  type FieldErrors,
  type ProductCommonValues,
} from './productForm';
import type { CharmDto } from '../../api/products';

type CharmErrors = FieldErrors<keyof ProductCommonValues | 'stock'>;

interface CharmFormProps {
  /** Produit à éditer ; absent ⇒ mode création. */
  product?: CharmDto;
  /** Appelé après une modification réussie (ex. retour au détail). */
  onSuccess?: () => void;
}

/** Formulaire breloque (création ou édition ; champ spécifique : stock entier). */
export const CharmForm = observer(function CharmForm({ product, onSuccess }: CharmFormProps) {
  const store = useProductAdminStore();
  const isEdit = Boolean(product);

  const [common, setCommon] = useState<ProductCommonValues>(
    product ? commonFromProduct(product) : emptyProductCommon
  );
  const [stock, setStock] = useState(product ? String(product.stock) : '');
  const [image, setImage] = useState<File | null>(null);
  const [imageError, setImageError] = useState<string | null>(null);
  const [errors, setErrors] = useState<CharmErrors>({});

  // Repart d'un état propre (pas de message résiduel d'une autre vue).
  useEffect(() => store.reset(), [store]);

  const patch = (p: Partial<ProductCommonValues>) => {
    setCommon((c) => ({ ...c, ...p }));
    store.reset();
  };

  const handleSubmit = async (e: FormEvent) => {
    e.preventDefault();
    const nextErrors: CharmErrors = validateCommon(common);

    const stockNum = toNumber(stock);
    if (Number.isNaN(stockNum)) nextErrors.stock = 'Stock requis.';
    else if (stockNum < 0) nextErrors.stock = 'Le stock doit être positif.';
    else if (!Number.isInteger(stockNum)) nextErrors.stock = 'Le stock doit être un entier.';

    setErrors(nextErrors);
    if (Object.keys(nextErrors).length > 0 || imageError) return;

    const payload = {
      name: common.name.trim(),
      description: common.description.trim(),
      color: common.color,
      url: common.url.trim() || null,
      cost: toNumber(common.cost),
      price: toNumber(common.price),
      stock: stockNum,
    };

    const ok =
      isEdit && product
        ? await store.updateCharm(product.id, payload, image)
        : await store.createCharm(payload, image);

    if (!ok) return;

    if (isEdit) {
      onSuccess?.();
    } else {
      setCommon(emptyProductCommon);
      setStock('');
      setImage(null);
      setImageError(null);
      setErrors({});
    }
  };

  const success =
    !isEdit && store.lastResult?.kind === 'charm' && store.lastResult.action === 'create'
      ? `Breloque « ${store.lastResult.name} » créée avec succès.`
      : null;

  return (
    <ProductFormShell
      onSubmit={handleSubmit}
      common={common}
      onCommonChange={patch}
      commonErrors={errors}
      submitLabel={isEdit ? 'Enregistrer les modifications' : 'Créer la breloque'}
      loading={store.loading}
      error={store.error}
      success={success}
    >
      <TextField
        label="Stock"
        type="number"
        step="1"
        min="0"
        value={stock}
        onChange={(v) => {
          setStock(v);
          store.reset();
        }}
        placeholder="0"
        error={errors.stock}
        required
      />
      <ImageUpload
        label={isEdit ? 'Remplacer la photo' : 'Photo'}
        value={image}
        onChange={(f) => {
          setImage(f);
          store.reset();
        }}
        error={imageError ?? undefined}
        onValidationError={setImageError}
      />
    </ProductFormShell>
  );
});
