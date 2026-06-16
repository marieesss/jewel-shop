import type { ReactNode } from 'react';
import { cn } from '../../lib/cn';

interface CardProps {
  children: ReactNode;
  className?: string;
}

/** Conteneur « carte » de la charte (fond blanc, coins arrondis, ombre douce). */
export function Card({ children, className }: CardProps) {
  return (
    <div
      className={cn(
        'rounded-[20px] border border-white/60 bg-white px-6 py-7 shadow-card',
        className
      )}
    >
      {children}
    </div>
  );
}
