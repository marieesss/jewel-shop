import type { ReactNode } from 'react';

interface FormInputProps {
  label: string;
  type?: string;
  value: string;
  onChange: (value: string) => void;
  placeholder?: string;
  icon?: ReactNode;
  delay?: number;
  required?: boolean;
}

export function FormInput({
  label,
  type = 'text',
  value,
  onChange,
  placeholder,
  icon,
  delay = 0,
  required,
}: FormInputProps) {
  const hasValue = value.length > 0;

  return (
    <div
      className="group animate-slide-up"
      style={{ animationDelay: `${delay}s` }}
    >
      <label className="mb-1.5 block font-body text-[11px] font-medium uppercase tracking-[0.08em] text-slate transition-colors group-focus-within:text-fuchsia">
        {label}
      </label>
      <div className="relative flex items-center">
        {icon && (
          <span className="pointer-events-none absolute left-3.5 flex items-center text-slate transition-colors group-focus-within:text-fuchsia">
            {icon}
          </span>
        )}
        <input
          type={type}
          value={value}
          required={required}
          placeholder={placeholder}
          onChange={(e) => onChange(e.target.value)}
          className={`w-full rounded-xl border-[1.5px] border-transparent bg-lin py-[13px] font-body text-sm font-normal text-encre outline-none transition-all focus:border-fuchsia focus:bg-white focus:shadow-focus ${
            icon ? 'pl-[42px] pr-4' : 'px-4'
          }`}
        />
        {hasValue && (
          <span className="absolute right-3.5 animate-fade-in text-sm text-fuchsia">
            ✓
          </span>
        )}
      </div>
    </div>
  );
}
