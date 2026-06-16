import { useId } from 'react';
import { cn } from '../../lib/cn';
import { FieldShell, controlClass } from './FieldShell';

export interface SelectOption {
  value: string;
  label: string;
}

interface SelectProps {
  label: string;
  value: string;
  onChange: (value: string) => void;
  options: SelectOption[];
  error?: string;
  hint?: string;
  required?: boolean;
  id?: string;
}

export function Select({
  label,
  value,
  onChange,
  options,
  error,
  hint,
  required,
  id,
}: SelectProps) {
  const generatedId = useId();
  const inputId = id ?? generatedId;

  return (
    <FieldShell label={label} htmlFor={inputId} error={error} hint={hint} required={required}>
      <select
        id={inputId}
        value={value}
        required={required}
        onChange={(e) => onChange(e.target.value)}
        className={cn(controlClass, 'cursor-pointer appearance-none', error && 'border-fuchsia')}
      >
        {options.map((opt) => (
          <option key={opt.value} value={opt.value}>
            {opt.label}
          </option>
        ))}
      </select>
    </FieldShell>
  );
}
