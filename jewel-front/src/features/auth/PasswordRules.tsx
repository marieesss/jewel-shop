import { cn } from '../../lib/cn';
import { passwordRules } from '../../lib/passwordPolicy';

interface PasswordRulesProps {
  password: string;
}

export function PasswordRules({ password }: PasswordRulesProps) {
  return (
    <ul aria-label="Règles du mot de passe" className="mt-2 flex flex-col gap-1">
      {passwordRules.map((rule) => {
        const isRespected = rule.test(password);
        return (
          <li
            key={rule.id}
            className={cn(
              'flex items-center gap-2 font-body text-[12px] transition-colors',
              isRespected ? 'text-fuchsia' : 'text-slate'
            )}
          >
            <span aria-hidden="true" className="w-3 text-center">
              {isRespected ? '✓' : '○'}
            </span>
            {rule.label}
            <span className="sr-only">{isRespected ? ' : respectée' : ' : non respectée'}</span>
          </li>
        );
      })}
    </ul>
  );
}
