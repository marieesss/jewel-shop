import { TextField } from '../../components/ui/TextField';
import { TextArea } from '../../components/ui/TextArea';
import { Select } from '../../components/ui/Select';
import { PRODUCT_COLORS, PRODUCT_COLOR_LABELS, type ProductColor } from '../../api/products';
import type { FieldErrors, ProductCommonValues } from './productForm';

interface Props {
  values: ProductCommonValues;
  onChange: (patch: Partial<ProductCommonValues>) => void;
  errors: FieldErrors<keyof ProductCommonValues>;
}

const COLOR_OPTIONS = PRODUCT_COLORS.map((c) => ({
  value: c,
  label: PRODUCT_COLOR_LABELS[c],
}));

/** Champs partagés par les formulaires breloque & chaîne. */
export function ProductCommonFields({ values, onChange, errors }: Props) {
  return (
    <>
      <TextField
        label="Nom"
        value={values.name}
        onChange={(v) => onChange({ name: v })}
        placeholder="Cœur scintillant"
        error={errors.name}
        required
      />

      <TextArea
        label="Description"
        value={values.description}
        onChange={(v) => onChange({ description: v })}
        placeholder="Décrivez le produit…"
        error={errors.description}
      />

      <Select
        label="Couleur"
        value={values.color}
        onChange={(v) => onChange({ color: v as ProductColor })}
        options={COLOR_OPTIONS}
        required
      />

      <TextField
        label="Lien (URL)"
        type="url"
        value={values.url}
        onChange={(v) => onChange({ url: v })}
        placeholder="https://exemple.com/produit"
        error={errors.url}
      />

      <div className="flex gap-3">
        <div className="flex-1">
          <TextField
            label="Coût (€)"
            type="number"
            step="0.01"
            min="0"
            value={values.cost}
            onChange={(v) => onChange({ cost: v })}
            placeholder="0.00"
            error={errors.cost}
            required
          />
        </div>
        <div className="flex-1">
          <TextField
            label="Prix (€)"
            type="number"
            step="0.01"
            min="0"
            value={values.price}
            onChange={(v) => onChange({ price: v })}
            placeholder="0.00"
            error={errors.price}
            required
          />
        </div>
      </div>
    </>
  );
}
