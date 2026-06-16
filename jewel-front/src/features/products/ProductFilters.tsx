import { Select } from '../../components/ui/Select';
import { TextField } from '../../components/ui/TextField';
import { Checkbox } from '../../components/ui/Checkbox';
import { Button } from '../../components/ui/Button';
import { PRODUCT_COLORS, PRODUCT_COLOR_LABELS, type ProductColor } from '../../api/products';

export interface ProductFilterValues {
  color: '' | ProductColor;
  minPrice: string;
  maxPrice: string;
  inStockOnly: boolean;
}

export const emptyFilters: ProductFilterValues = {
  color: '',
  minPrice: '',
  maxPrice: '',
  inStockOnly: false,
};

interface ProductFiltersProps {
  value: ProductFilterValues;
  onChange: (patch: Partial<ProductFilterValues>) => void;
  onReset: () => void;
  /** Affiche le filtre « en stock » (breloques uniquement). */
  showStock?: boolean;
}

const COLOR_OPTIONS = [
  { value: '', label: 'Toutes les couleurs' },
  ...PRODUCT_COLORS.map((c) => ({ value: c, label: PRODUCT_COLOR_LABELS[c] })),
];

/** Panneau de filtres latéral réutilisable pour le listing produit. */
export function ProductFilters({ value, onChange, onReset, showStock }: ProductFiltersProps) {
  return (
    <div className="rounded-[18px] border border-lin bg-white p-4 shadow-card">
      <p className="mb-4 font-comfortaa text-base font-light text-storm">Filtres</p>

      <div className="flex flex-col gap-4">
        <Select
          label="Couleur"
          value={value.color}
          onChange={(v) => onChange({ color: v as '' | ProductColor })}
          options={COLOR_OPTIONS}
          required
        />
        <TextField
          label="Prix min (€)"
          type="number"
          min="0"
          step="0.01"
          value={value.minPrice}
          onChange={(v) => onChange({ minPrice: v })}
          placeholder="0"
        />
        <TextField
          label="Prix max (€)"
          type="number"
          min="0"
          step="0.01"
          value={value.maxPrice}
          onChange={(v) => onChange({ maxPrice: v })}
          placeholder="—"
        />
        {showStock && (
          <Checkbox
            label="En stock uniquement"
            checked={value.inStockOnly}
            onChange={(c) => onChange({ inStockOnly: c })}
          />
        )}

        <Button variant="ghost" className="justify-start px-0 py-1" onClick={onReset}>
          Réinitialiser les filtres
        </Button>
      </div>
    </div>
  );
}
