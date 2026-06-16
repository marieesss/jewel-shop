import { useId, type InputHTMLAttributes } from 'react';
import { cn } from '../../lib/cn';
import { FieldShell, controlClass } from './FieldShell';

interface TextFieldProps
  extends Omit<InputHTMLAttributes<HTMLInputElement>, 'onChange' | 'value'> {
  label: string;
  value: string;
  onChange: (value: string) => void;
  error?: string;
  hint?: string;
}

export function TextField({
  label,
  value,
  onChange,
  error,
  hint,
  required,
  className,
  id,
  ...rest
}: TextFieldProps) {
  const generatedId = useId();
  const inputId = id ?? generatedId;

  return (
    <FieldShell label={label} htmlFor={inputId} error={error} hint={hint} required={required}>
      <input
        id={inputId}
        value={value}
        required={required}
        onChange={(e) => onChange(e.target.value)}
        className={cn(controlClass, error && 'border-fuchsia', className)}
        {...rest}
      />
    </FieldShell>
  );
}
