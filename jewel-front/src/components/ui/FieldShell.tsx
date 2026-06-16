import type { ReactNode } from 'react';
import { cn } from '../../lib/cn';

interface FieldShellProps {
  label: string;
  htmlFor?: string;
  error?: string;
  hint?: string;
  required?: boolean;
  className?: string;
  children: ReactNode;
}

/** Habillage commun d'un champ : label, indication, message d'erreur. */
export function FieldShell({
  label,
  htmlFor,
  error,
  hint,
  required,
  className,
  children,
}: FieldShellProps) {
  return (
    <div className={cn('group', className)}>
      <label
        htmlFor={htmlFor}
        className="mb-1.5 block font-body text-[11px] font-medium uppercase tracking-[0.08em] text-slate transition-colors group-focus-within:text-fuchsia"
      >
        {label}
        {!required && <span className="ml-1 normal-case tracking-normal text-slate/60">(optionnel)</span>}
      </label>
      {children}
      {hint && !error && <p className="mt-1 font-body text-[11px] text-slate/80">{hint}</p>}
      {error && <p className="mt-1 font-body text-[11px] font-medium text-fuchsia">{error}</p>}
    </div>
  );
}

/** Classes partagées par les contrôles de saisie (input, textarea, select). */
export const controlClass =
  'w-full rounded-xl border-[1.5px] border-transparent bg-lin px-4 py-[13px] font-body text-sm text-encre outline-none transition-all placeholder:text-slate/50 focus:border-fuchsia focus:bg-white focus:shadow-focus';
