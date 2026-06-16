import { useState, type FormEvent } from 'react';
import { observer } from 'mobx-react-lite';
import { useProductAdminStore } from '../../stores/context';
import { TextField } from '../../components/ui/TextField';
import { ImageUpload } from '../../components/ui/ImageUpload';
import { ProductFormShell } from './ProductFormShell';
import {
  emptyProductCommon,
  toNumber,
  validateCommon,
  type FieldErrors,
  type ProductCommonValues,
} from './productForm';

type CharmErrors = FieldErrors<keyof ProductCommonValues | 'stock'>;

/** Formulaire de création d'une breloque (champ spécifique : stock entier). */
export const CharmForm = observer(function CharmForm() {
  const store = useProductAdminStore();

  const [common, setCommon] = useState<ProductCommonValues>(emptyProductCommon);
  const [stock, setStock] = useState('');
  const [image, setImage] = useState<File | null>(null);
  const [imageError, setImageError] = useState<string | null>(null);
  const [errors, setErrors] = useState<CharmErrors>({});

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

    const ok = await store.createCharm(
      {
        name: common.name.trim(),
        description: common.description.trim(),
        color: common.color,
        url: common.url.trim() || null,
        cost: toNumber(common.cost),
        price: toNumber(common.price),
        stock: stockNum,
      },
      image
    );

    if (ok) {
      setCommon(emptyProductCommon);
      setStock('');
      setImage(null);
      setImageError(null);
      setErrors({});
    }
  };

  const success =
    store.lastCreated?.kind === 'charm'
      ? `Breloque « ${store.lastCreated.name} » créée avec succès.`
      : null;

  return (
    <ProductFormShell
      onSubmit={handleSubmit}
      common={common}
      onCommonChange={patch}
      commonErrors={errors}
      submitLabel="Créer la breloque"
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
        label="Photo"
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
