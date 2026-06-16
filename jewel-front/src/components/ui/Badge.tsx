import type { ReactNode } from 'react';
import { cn } from '../../lib/cn';

type Tone = 'gold' | 'silver' | 'neutral';

interface BadgeProps {
  tone?: Tone;
  children: ReactNode;
  className?: string;
}

const TONES: Record<Tone, string> = {
  gold: 'bg-[#f7ecd2] text-[#9a7b1f]',
  silver: 'bg-[#e9edf1] text-[#5d7081]',
  neutral: 'bg-lin text-slate',
};

export function Badge({ tone = 'neutral', children, className }: BadgeProps) {
  return (
    <span
      className={cn(
        'inline-flex items-center rounded-full px-2.5 py-1 font-body text-[11px] font-medium',
        TONES[tone],
        className
      )}
    >
      {children}
    </span>
  );
}
