import { useEffect, useState, type FormEvent } from 'react';
import { observer } from 'mobx-react-lite';
import { useProductAdminStore } from '../../stores/context';
import { TextField } from '../../components/ui/TextField';
import { ImageUpload } from '../../components/ui/ImageUpload';
import { ProductFormShell } from './ProductFormShell';
import { emptyProductCommon, type ProductCommonValues } from './productForm';
import {
  chainCommonFromProduct,
  chainSpecificFromProduct,
  emptyChainSpecific,
  toCreateChainPayload,
  toUpdateChainPayload,
  validateChain,
  type ChainErrors,
  type ChainSpecificValues,
} from './chainFormValues';
import type { ChainDto } from '../../api/products';

const METER_PRICE_LABELS = { cost: 'Coût au mètre (€)', price: 'Prix au mètre (€)' };

interface ChainFormProps {
  /** Produit à éditer ; absent ⇒ mode création. */
  product?: ChainDto;
  /** Appelé après une modification réussie (ex. retour au détail). */
  onSuccess?: () => void;
}

/**
 * Formulaire chaîne (création ou édition) : prix au mètre, fermoir, et longueur du premier
 * rouleau à la création. Les rouleaux suivants se gèrent sur la fiche de la chaîne.
 */
export const ChainForm = observer(function ChainForm({ product, onSuccess }: ChainFormProps) {
  const store = useProductAdminStore();
  const isEdit = Boolean(product);

  const [common, setCommon] = useState<ProductCommonValues>(
    product ? chainCommonFromProduct(product) : emptyProductCommon
  );
  const [specific, setSpecific] = useState<ChainSpecificValues>(
    product ? chainSpecificFromProduct(product) : emptyChainSpecific
  );
  const [image, setImage] = useState<File | null>(null);
  const [imageError, setImageError] = useState<string | null>(null);
  const [errors, setErrors] = useState<ChainErrors>({});

  // Repart d'un état propre (pas de message résiduel d'une autre vue).
  useEffect(() => store.reset(), [store]);

  const patch = (p: Partial<ProductCommonValues>) => {
    setCommon((c) => ({ ...c, ...p }));
    store.reset();
  };

  const patchSpecific = (p: Partial<ChainSpecificValues>) => {
    setSpecific((s) => ({ ...s, ...p }));
    store.reset();
  };

  const handleSubmit = async (e: FormEvent) => {
    e.preventDefault();
    const nextErrors = validateChain(common, specific, !isEdit);

    setErrors(nextErrors);
    if (Object.keys(nextErrors).length > 0 || imageError) return;

    const ok =
      isEdit && product
        ? await store.updateChain(product.id, toUpdateChainPayload(common, specific), image)
        : await store.createChain(toCreateChainPayload(common, specific), image);

    if (!ok) return;

    if (isEdit) {
      onSuccess?.();
    } else {
      setCommon(emptyProductCommon);
      setSpecific(emptyChainSpecific);
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
      priceLabels={METER_PRICE_LABELS}
      submitLabel={isEdit ? 'Enregistrer les modifications' : 'Créer la chaîne'}
      loading={store.loading}
      error={store.error}
      success={success}
    >
      <div className="flex gap-3">
        <div className="flex-1">
          <TextField
            label="Coût du fermoir (€)"
            type="number"
            step="0.01"
            min="0"
            value={specific.claspCost}
            onChange={(v) => patchSpecific({ claspCost: v })}
            placeholder="0.00"
            error={errors.claspCost}
            required
          />
        </div>
        <div className="flex-1">
          <TextField
            label="Prix du fermoir (€)"
            type="number"
            step="0.01"
            min="0"
            value={specific.claspPrice}
            onChange={(v) => patchSpecific({ claspPrice: v })}
            placeholder="0.00"
            error={errors.claspPrice}
            required
          />
        </div>
      </div>
      {!isEdit && (
        <TextField
          label="Longueur du rouleau (cm)"
          type="number"
          step="1"
          min="1"
          value={specific.initialRollLengthCm}
          onChange={(v) => patchSpecific({ initialRollLengthCm: v })}
          placeholder="500"
          error={errors.initialRollLengthCm}
          required
        />
      )}
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
