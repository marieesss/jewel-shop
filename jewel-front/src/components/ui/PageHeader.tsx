interface PageHeaderProps {
  title: string;
  subtitle?: string;
}

/** En-tête de page standard pour la zone admin. */
export function PageHeader({ title, subtitle }: PageHeaderProps) {
  return (
    <div className="mb-7">
      <h1 className="font-comfortaa text-[30px] font-light leading-tight text-storm">{title}</h1>
      {subtitle && <p className="mt-1 font-display text-base italic text-slate">{subtitle}</p>}
    </div>
  );
}
