import type { ReactNode } from 'react';
import { cn } from '../../lib/cn';

type Variant = 'error' | 'success';

interface AlertProps {
  variant?: Variant;
  children: ReactNode;
  className?: string;
}

const VARIANTS: Record<Variant, string> = {
  error: 'border-poudre bg-blush text-fuchsia',
  success: 'border-mist bg-[#eef6ff] text-storm',
};

export function Alert({ variant = 'error', children, className }: AlertProps) {
  return (
    <div
      role="alert"
      className={cn(
        'animate-fade-in rounded-xl border px-3.5 py-[11px] font-body text-[13px] font-medium',
        VARIANTS[variant],
        className
      )}
    >
      {children}
    </div>
  );
}
