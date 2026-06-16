import type { ButtonHTMLAttributes, ReactNode } from 'react';
import { cn } from '../../lib/cn';

type Variant = 'primary' | 'secondary' | 'ghost';

interface ButtonProps extends ButtonHTMLAttributes<HTMLButtonElement> {
  variant?: Variant;
  fullWidth?: boolean;
  loading?: boolean;
  leftIcon?: ReactNode;
}

const VARIANTS: Record<Variant, string> = {
  primary:
    'bg-storm text-white shadow-btn hover:-translate-y-0.5 hover:shadow-btn-hover disabled:hover:translate-y-0 disabled:hover:shadow-btn',
  secondary:
    'bg-white text-storm border-[1.5px] border-lin hover:border-fuchsia hover:bg-blush',
  ghost: 'bg-transparent text-slate hover:text-fuchsia',
};

export function Button({
  variant = 'primary',
  fullWidth,
  loading,
  leftIcon,
  disabled,
  className,
  children,
  ...rest
}: ButtonProps) {
  return (
    <button
      disabled={disabled || loading}
      className={cn(
        'inline-flex items-center justify-center gap-2 rounded-xl px-6 py-[13px] font-body text-sm font-medium tracking-[0.04em] transition-all duration-300 outline-none',
        'disabled:cursor-not-allowed disabled:opacity-70',
        VARIANTS[variant],
        fullWidth && 'w-full',
        className
      )}
      {...rest}
    >
      {leftIcon}
      {children}
    </button>
  );
}
