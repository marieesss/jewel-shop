import type { ReactNode } from 'react';

interface DetailRowProps {
  label: string;
  children: ReactNode;
}

/** Ligne « libellé / valeur » réutilisable pour les vues de détail. */
export function DetailRow({ label, children }: DetailRowProps) {
  return (
    <div className="flex items-baseline justify-between gap-4 border-b border-lin py-2.5 last:border-b-0">
      <span className="font-body text-[11px] font-medium uppercase tracking-[0.08em] text-slate">
        {label}
      </span>
      <span className="text-right font-body text-sm text-encre">{children}</span>
    </div>
  );
}
