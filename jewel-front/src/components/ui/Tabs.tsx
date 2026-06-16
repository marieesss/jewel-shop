import type { ReactNode } from 'react';
import { cn } from '../../lib/cn';

export interface TabItem<T extends string> {
  value: T;
  label: string;
  icon?: ReactNode;
}

interface TabsProps<T extends string> {
  items: TabItem<T>[];
  value: T;
  onChange: (value: T) => void;
  className?: string;
}

/** Bascule onglet réutilisable (style « pill » de la charte). */
export function Tabs<T extends string>({ items, value, onChange, className }: TabsProps<T>) {
  return (
    <div className={cn('flex gap-1 rounded-[14px] bg-lin p-1', className)} role="tablist">
      {items.map((item) => {
        const active = item.value === value;
        return (
          <button
            key={item.value}
            type="button"
            role="tab"
            aria-selected={active}
            onClick={() => onChange(item.value)}
            className={cn(
              'flex flex-1 items-center justify-center gap-2 rounded-[11px] py-[11px] font-body text-[13px] font-medium tracking-[0.03em] transition-all duration-300',
              active ? 'bg-white text-fuchsia shadow-toggle' : 'bg-transparent text-slate hover:text-storm'
            )}
          >
            {item.icon}
            {item.label}
          </button>
        );
      })}
    </div>
  );
}
