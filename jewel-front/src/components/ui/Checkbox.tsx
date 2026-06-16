import { useId } from 'react';

interface CheckboxProps {
  label: string;
  checked: boolean;
  onChange: (checked: boolean) => void;
}

/** Case à cocher réutilisable, alignée sur la charte. */
export function Checkbox({ label, checked, onChange }: CheckboxProps) {
  const id = useId();
  return (
    <label htmlFor={id} className="inline-flex cursor-pointer items-center gap-2 select-none">
      <input
        id={id}
        type="checkbox"
        checked={checked}
        onChange={(e) => onChange(e.target.checked)}
        className="h-4 w-4 cursor-pointer rounded border-slate/40 text-fuchsia accent-fuchsia"
      />
      <span className="font-body text-sm text-encre">{label}</span>
    </label>
  );
}
