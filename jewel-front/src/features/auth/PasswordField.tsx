import { FormInput } from '../../components/FormInput';
import { LockIcon } from '../../components/icons';
import { PasswordRules } from './PasswordRules';

interface PasswordFieldProps {
  label: string;
  value: string;
  onChange: (value: string) => void;
  /** Mot de passe affiché en clair. L'état reste chez le parent, qui le partage avec la confirmation. */
  visible: boolean;
  onToggleVisible: () => void;
  delay?: number;
  showRules?: boolean;
}

/** Champ mot de passe avec le bouton « Afficher/Masquer » et, si demandé, les règles du mot de passe. */
export function PasswordField({
  label,
  value,
  onChange,
  visible,
  onToggleVisible,
  delay,
  showRules = false,
}: PasswordFieldProps) {
  return (
    <div>
      <FormInput
        label={label}
        type={visible ? 'text' : 'password'}
        value={value}
        onChange={onChange}
        placeholder="••••••••"
        icon={<LockIcon />}
        delay={delay}
        required
      />
      <button
        type="button"
        onClick={onToggleVisible}
        className="mt-1 py-0.5 font-body text-[11px] text-slate transition-colors hover:text-fuchsia"
      >
        {visible ? 'Masquer' : 'Afficher'}
      </button>
      {showRules && <PasswordRules password={value} />}
    </div>
  );
}
