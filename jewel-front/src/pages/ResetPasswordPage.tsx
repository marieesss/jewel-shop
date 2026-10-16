import { useState, type FormEvent } from 'react';
import { Link, useNavigate, useSearchParams } from 'react-router-dom';
import { resetPassword } from '../api/auth';
import { extractApiError } from '../api/errors';
import { validatePassword } from '../lib/passwordPolicy';
import type { LoginMessageState } from '../lib/loginMessage';
import {
  INVALID_RESET_TOKEN_MESSAGE,
  PASSWORD_MISMATCH_MESSAGE,
  PASSWORD_RESET_SUCCESS_MESSAGE,
  isInvalidResetTokenError,
} from '../lib/passwordReset';
import { AuthHeader } from '../features/auth/AuthHeader';
import { AuthLayout } from '../features/auth/AuthLayout';
import { PasswordField } from '../features/auth/PasswordField';
import { Alert } from '../components/ui/Alert';
import { Button } from '../components/ui/Button';
import { FormInput } from '../components/FormInput';
import { LockIcon } from '../components/icons';

const linkClassName =
  'font-body text-[13px] font-semibold text-fuchsia underline underline-offset-[3px] transition-opacity hover:opacity-70';

function InvalidResetLink() {
  return (
    <>
      <Alert>{INVALID_RESET_TOKEN_MESSAGE}</Alert>
      <p className="mt-6 text-center">
        <Link to="/forgot-password" className={linkClassName}>
          Demander un nouveau lien
        </Link>
      </p>
    </>
  );
}

export function ResetPasswordPage() {
  const navigate = useNavigate();
  const [searchParams] = useSearchParams();
  const token = searchParams.get('token') ?? '';

  const [password, setPassword] = useState('');
  const [confirmPassword, setConfirmPassword] = useState('');
  const [showPassword, setShowPassword] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [loading, setLoading] = useState(false);
  const [tokenRejected, setTokenRejected] = useState(false);

  const handleSubmit = async (e: FormEvent) => {
    e.preventDefault();
    setError(null);

    const [firstPolicyError] = validatePassword(password);
    if (firstPolicyError) {
      setError(firstPolicyError);
      return;
    }
    if (password !== confirmPassword) {
      setError(PASSWORD_MISMATCH_MESSAGE);
      return;
    }

    setLoading(true);
    try {
      await resetPassword(token, password);
      const state: LoginMessageState = { message: PASSWORD_RESET_SUCCESS_MESSAGE };
      navigate('/login', { replace: true, state });
    } catch (err) {
      if (isInvalidResetTokenError(err)) {
        setTokenRejected(true);
      } else {
        setError(extractApiError(err));
      }
      setLoading(false);
    }
  };

  const isLinkInvalid = token.trim() === '' || tokenRejected;

  return (
    <AuthLayout>
      <AuthHeader
        title="Nouveau mot de passe"
        subtitle="Choisissez le mot de passe de votre compte"
      />

      {isLinkInvalid ? (
        <InvalidResetLink />
      ) : (
        <form
          onSubmit={handleSubmit}
          noValidate
          className="rounded-[20px] border border-white/60 bg-white/70 px-6 py-7 shadow-card backdrop-blur-[20px]"
        >
          <div className="flex flex-col gap-[18px]">
            <PasswordField
              label="Nouveau mot de passe"
              value={password}
              onChange={setPassword}
              visible={showPassword}
              onToggleVisible={() => setShowPassword(!showPassword)}
              showRules
            />

            <FormInput
              label="Confirmer le mot de passe"
              type={showPassword ? 'text' : 'password'}
              value={confirmPassword}
              onChange={setConfirmPassword}
              placeholder="••••••••"
              icon={<LockIcon />}
              delay={0.05}
              required
            />
          </div>

          {error && <Alert className="mt-[18px]">{error}</Alert>}

          <Button type="submit" fullWidth loading={loading} className="mt-6">
            {loading ? 'Veuillez patienter…' : 'Modifier le mot de passe'}
          </Button>
        </form>
      )}
    </AuthLayout>
  );
}
