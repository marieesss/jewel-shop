interface AuthHeaderProps {
  title: string;
  subtitle: string;
}

export function AuthHeader({ title, subtitle }: AuthHeaderProps) {
  return (
    <div className="mb-7">
      <h2 className="mb-1.5 font-comfortaa text-[36px] font-light leading-[1.15] text-storm">
        {title}
      </h2>
      <p className="font-display text-base italic text-slate">{subtitle}</p>
    </div>
  );
}
