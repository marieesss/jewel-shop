import { cn } from '../../lib/cn';

interface SpinnerProps {
  className?: string;
  label?: string;
}

/** Indicateur de chargement simple, centré. */
export function Spinner({ className, label }: SpinnerProps) {
  return (
    <div className={cn('flex flex-col items-center justify-center gap-3 py-12', className)}>
      <span className="h-7 w-7 animate-spin rounded-full border-2 border-lin border-t-fuchsia" />
      {label && <p className="font-body text-[13px] text-slate">{label}</p>}
    </div>
  );
}
