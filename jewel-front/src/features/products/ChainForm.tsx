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
import type { ChainDto } from '../../api/products';

type ChainErrors = FieldErrors<keyof ProductCommonValues | 'length'>;

interface ChainFormProps {
  /** Produit à éditer ; absent ⇒ mode création. */
  product?: ChainDto;
  /** Appelé après une modification réussie (ex. retour au détail). */
  onSuccess?: () => void;
}

/** Formulaire chaîne (création ou édition ; champ spécifique : longueur > 0, en cm). */
export const ChainForm = observer(function ChainForm({ product, onSuccess }: ChainFormProps) {
  const store = useProductAdminStore();
  const isEdit = Boolean(product);

  const [common, setCommon] = useState<ProductCommonValues>(
    product ? commonFromProduct(product) : emptyProductCommon
  );
  const [length, setLength] = useState(product ? String(product.length) : '');
  const [image, setImage] = useState<File | null>(null);
  const [imageError, setImageError] = useState<string | null>(null);
  const [errors, setErrors] = useState<ChainErrors>({});

  // Repart d'un état propre (pas de message résiduel d'une autre vue).
  useEffect(() => store.reset(), [store]);

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

    const payload = {
      name: common.name.trim(),
      description: common.description.trim(),
      color: common.color,
      url: common.url.trim() || null,
      cost: toNumber(common.cost),
      price: toNumber(common.price),
      length: lengthNum,
    };

    const ok =
      isEdit && product
        ? await store.updateChain(product.id, payload, image)
        : await store.createChain(payload, image);

    if (!ok) return;

    if (isEdit) {
      onSuccess?.();
    } else {
      setCommon(emptyProductCommon);
      setLength('');
      setImage(null);
      setImageError(null);
      setErrors({});
    }
  };

  const success =
    !isEdit && store.lastResult?.kind === 'chain' && store.lastResult.action === 'create'
      ? `Chaîne « ${store.lastResult.name} » créée avec succès.`
      : null;

  return (
    <ProductFormShell
      onSubmit={handleSubmit}
      common={common}
      onCommonChange={patch}
      commonErrors={errors}
      submitLabel={isEdit ? 'Enregistrer les modifications' : 'Créer la chaîne'}
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
