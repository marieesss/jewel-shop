import type { ReactNode } from 'react';

interface EmptyStateProps {
  title: string;
  description?: string;
  action?: ReactNode;
}

/** Bloc « aucun résultat » réutilisable. */
export function EmptyState({ title, description, action }: EmptyStateProps) {
  return (
    <div className="flex flex-col items-center justify-center gap-2 rounded-[20px] border border-dashed border-slate/30 bg-white/50 py-14 text-center">
      <p className="font-comfortaa text-lg font-light text-storm">{title}</p>
      {description && <p className="max-w-sm font-body text-[13px] text-slate">{description}</p>}
      {action && <div className="mt-2">{action}</div>}
    </div>
  );
}
