import { useState, type FormEvent } from 'react';
import { Link } from 'react-router-dom';
import { forgotPassword } from '../api/auth';
import { extractApiError } from '../api/errors';
import { validateEmail } from '../lib/emailValidation';
import { FORGOT_PASSWORD_SENT_MESSAGE } from '../lib/passwordReset';
import { AuthLayout } from '../features/auth/AuthLayout';
import { Alert } from '../components/ui/Alert';
import { FormInput } from '../components/FormInput';
import { MailIcon } from '../components/icons';

export function ForgotPasswordPage() {
  const [email, setEmail] = useState('');
  const [error, setError] = useState<string | null>(null);
  const [loading, setLoading] = useState(false);
  const [sent, setSent] = useState(false);

  const handleSubmit = async (e: FormEvent) => {
    e.preventDefault();
    setError(null);

    const [firstEmailError] = validateEmail(email);
    if (firstEmailError) {
      setError(firstEmailError);
      return;
    }

    setLoading(true);
    try {
      await forgotPassword(email);
      setSent(true);
    } catch (err) {
      setError(extractApiError(err));
    } finally {
      setLoading(false);
    }
  };

  return (
    <AuthLayout>
      <div className="mb-7">
        <h2 className="mb-1.5 font-comfortaa text-[36px] font-light leading-[1.15] text-storm">
          Mot de passe oublié
        </h2>
        <p className="font-display text-base italic text-slate">
          Recevez un lien pour choisir un nouveau mot de passe
        </p>
      </div>

      {sent ? (
        <Alert variant="success">{FORGOT_PASSWORD_SENT_MESSAGE}</Alert>
      ) : (
        <form
          onSubmit={handleSubmit}
          noValidate
          className="rounded-[20px] border border-white/60 bg-white/70 px-6 py-7 shadow-card backdrop-blur-[20px]"
        >
          <FormInput
            label="Email"
            type="email"
            value={email}
            onChange={setEmail}
            placeholder="marie@exemple.com"
            icon={<MailIcon />}
            required
          />

          {error && <Alert className="mt-[18px]">{error}</Alert>}

          <button
            type="submit"
            disabled={loading}
            className="relative mt-6 w-full overflow-hidden rounded-xl bg-storm py-[14px] font-body text-sm font-medium tracking-[0.04em] text-white shadow-btn transition-all duration-300 hover:-translate-y-0.5 hover:shadow-btn-hover disabled:cursor-wait disabled:opacity-75 disabled:hover:translate-y-0 disabled:hover:shadow-btn"
          >
            {loading ? 'Veuillez patienter…' : 'Envoyer le lien'}
          </button>
        </form>
      )}

      <p className="mt-6 text-center">
        <Link
          to="/login"
          className="font-body text-[13px] font-semibold text-fuchsia underline underline-offset-[3px] transition-opacity hover:opacity-70"
        >
          Retour à la connexion
        </Link>
      </p>
    </AuthLayout>
  );
}
