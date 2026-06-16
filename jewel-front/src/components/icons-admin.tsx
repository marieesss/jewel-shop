import type { SVGProps } from 'react';

const base = (props: SVGProps<SVGSVGElement>) => ({
  width: 18,
  height: 18,
  viewBox: '0 0 18 18',
  fill: 'none',
  stroke: 'currentColor',
  strokeWidth: 1.5,
  strokeLinecap: 'round' as const,
  strokeLinejoin: 'round' as const,
  ...props,
});

export function DashboardIcon(props: SVGProps<SVGSVGElement>) {
  return (
    <svg {...base(props)}>
      <rect x="2" y="2" width="6" height="6" rx="1.5" />
      <rect x="10" y="2" width="6" height="6" rx="1.5" />
      <rect x="2" y="10" width="6" height="6" rx="1.5" />
      <rect x="10" y="10" width="6" height="6" rx="1.5" />
    </svg>
  );
}

export function GemIcon(props: SVGProps<SVGSVGElement>) {
  return (
    <svg {...base(props)}>
      <path d="M3 7L9 16L15 7L12 2H6L3 7Z" />
      <path d="M3 7H15" />
      <path d="M6 2L9 7L12 2" />
      <path d="M9 7V16" opacity="0.5" />
    </svg>
  );
}

export function ChainIcon(props: SVGProps<SVGSVGElement>) {
  return (
    <svg {...base(props)}>
      <path d="M7 11L11 7" />
      <path d="M9 5L10 4A2.8 2.8 0 0 1 14 8L13 9" />
      <path d="M9 13L8 14A2.8 2.8 0 0 1 4 10L5 9" />
    </svg>
  );
}

export function UsersIcon(props: SVGProps<SVGSVGElement>) {
  return (
    <svg {...base(props)}>
      <circle cx="7" cy="6" r="2.5" />
      <path d="M2.5 15C2.5 12 4.5 10.5 7 10.5C9.5 10.5 11.5 12 11.5 15" />
      <path d="M12 4.2A2.5 2.5 0 0 1 12 9.3" opacity="0.7" />
      <path d="M13 10.7C14.7 11.1 16 12.4 16 15" opacity="0.7" />
    </svg>
  );
}

export function LogoutIcon(props: SVGProps<SVGSVGElement>) {
  return (
    <svg {...base(props)}>
      <path d="M7 2H4A1.5 1.5 0 0 0 2.5 3.5V14.5A1.5 1.5 0 0 0 4 16H7" />
      <path d="M11 12L15 9L11 6" />
      <path d="M15 9H7" />
    </svg>
  );
}

export function MenuIcon(props: SVGProps<SVGSVGElement>) {
  return (
    <svg {...base(props)}>
      <path d="M3 5H15" />
      <path d="M3 9H15" />
      <path d="M3 13H15" />
    </svg>
  );
}
