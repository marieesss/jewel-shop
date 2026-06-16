import { useId, type TextareaHTMLAttributes } from 'react';
import { cn } from '../../lib/cn';
import { FieldShell, controlClass } from './FieldShell';

interface TextAreaProps
  extends Omit<TextareaHTMLAttributes<HTMLTextAreaElement>, 'onChange' | 'value'> {
  label: string;
  value: string;
  onChange: (value: string) => void;
  error?: string;
  hint?: string;
}

export function TextArea({
  label,
  value,
  onChange,
  error,
  hint,
  required,
  rows = 4,
  className,
  id,
  ...rest
}: TextAreaProps) {
  const generatedId = useId();
  const inputId = id ?? generatedId;

  return (
    <FieldShell label={label} htmlFor={inputId} error={error} hint={hint} required={required}>
      <textarea
        id={inputId}
        value={value}
        rows={rows}
        required={required}
        onChange={(e) => onChange(e.target.value)}
        className={cn(controlClass, 'resize-y', error && 'border-fuchsia', className)}
        {...rest}
      />
    </FieldShell>
  );
}
