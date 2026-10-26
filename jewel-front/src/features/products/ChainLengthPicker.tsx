import { useId } from 'react';
import type { ChainLengthPriceDto } from '../../api/products';
import { cn } from '../../lib/cn';
import { LENGTH_UNAVAILABLE_LABEL, formatLengthOption, isSelectableLength } from './chainPrice';

interface ChainLengthPickerProps {
  lengths: ChainLengthPriceDto[];
  selectedLengthCm: number | null;
  onSelect: (lengthCm: number) => void;
}

/** Choix de la longueur d'une chaîne. Une longueur indisponible est grisée et non sélectionnable. */
export function ChainLengthPicker({ lengths, selectedLengthCm, onSelect }: ChainLengthPickerProps) {
  const name = useId();

  return (
    <fieldset className="mb-5">
      <legend className="mb-2 font-body text-[11px] font-medium uppercase tracking-[0.08em] text-slate">
        Longueur
      </legend>
      <div className="flex flex-col gap-2">
        {lengths.map((length) => {
          const selectable = isSelectableLength(length);
          const selected = selectable && length.lengthCm === selectedLengthCm;
          return (
            <label
              key={length.lengthCm}
              className={cn(
                'flex items-center gap-3 rounded-xl border-[1.5px] px-4 py-2.5 font-body text-sm transition-colors',
                selected ? 'border-fuchsia text-storm' : 'border-lin text-encre',
                selectable ? 'cursor-pointer hover:border-fuchsia' : 'cursor-not-allowed opacity-50'
              )}
            >
              <input
                type="radio"
                name={name}
                value={length.lengthCm}
                checked={selected}
                disabled={!selectable}
                onChange={() => onSelect(length.lengthCm)}
                className="h-4 w-4 accent-fuchsia disabled:cursor-not-allowed"
              />
              <span className="flex-1">{formatLengthOption(length)}</span>
              {!selectable && <span className="text-slate">{LENGTH_UNAVAILABLE_LABEL}</span>}
            </label>
          );
        })}
      </div>
    </fieldset>
  );
}
