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

type ChainErrors = FieldErrors<keyof ProductCommonValues | 'length'>;

/** Formulaire de création d'une chaîne (champ spécifique : longueur > 0, en cm). */
export const ChainForm = observer(function ChainForm() {
  const store = useProductAdminStore();

  const [common, setCommon] = useState<ProductCommonValues>(emptyProductCommon);
  const [length, setLength] = useState('');
  const [image, setImage] = useState<File | null>(null);
  const [imageError, setImageError] = useState<string | null>(null);
  const [errors, setErrors] = useState<ChainErrors>({});

  const patch = (p: Partial<ProductCommonValues>) => {
    setCommon((c) => ({ ...c, ...p }));
    store.reset();
  };

  const handleSubmit = async (e: FormEvent) => {
    e.preventDefault();
    const nextErrors: ChainErrors = validateCommon(common);

    const lengthNum = toNumber(length);
    if (Number.isNaN(lengthNum)) nextErrors.length = 'Longueur requise.';
    else if (lengthNum <= 0) nextErrors.length = 'La longueur doit être supérieure à 0.';

    setErrors(nextErrors);
    if (Object.keys(nextErrors).length > 0 || imageError) return;

    const ok = await store.createChain(
      {
        name: common.name.trim(),
        description: common.description.trim(),
        color: common.color,
        url: common.url.trim() || null,
        cost: toNumber(common.cost),
        price: toNumber(common.price),
        length: lengthNum,
      },
      image
    );

    if (ok) {
      setCommon(emptyProductCommon);
      setLength('');
      setImage(null);
      setImageError(null);
      setErrors({});
    }
  };

  const success =
    store.lastCreated?.kind === 'chain'
      ? `Chaîne « ${store.lastCreated.name} » créée avec succès.`
      : null;

  return (
    <ProductFormShell
      onSubmit={handleSubmit}
      common={common}
      onCommonChange={patch}
      commonErrors={errors}
      submitLabel="Créer la chaîne"
      loading={store.loading}
      error={store.error}
      success={success}
    >
      <TextField
        label="Longueur (cm)"
        type="number"
        step="0.1"
        min="0"
        value={length}
        onChange={(v) => {
          setLength(v);
          store.reset();
        }}
        placeholder="45"
        error={errors.length}
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
