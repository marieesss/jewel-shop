import { useState, type FormEvent } from 'react';
import { Link } from 'react-router-dom';
import { forgotPassword } from '../api/auth';
import { extractApiError } from '../api/errors';
import { validateEmail } from '../lib/emailValidation';
import { FORGOT_PASSWORD_SENT_MESSAGE } from '../lib/passwordReset';
import { AuthHeader } from '../features/auth/AuthHeader';
import { AuthLayout } from '../features/auth/AuthLayout';
import { Alert } from '../components/ui/Alert';
import { Button } from '../components/ui/Button';
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

    // L'API reçoit l'email nettoyé : on valide donc cette même valeur.
    const trimmedEmail = email.trim();
    const [firstEmailError] = validateEmail(trimmedEmail);
    if (firstEmailError) {
      setError(firstEmailError);
      return;
    }

    setLoading(true);
    try {
      await forgotPassword(trimmedEmail);
      setSent(true);
    } catch (err) {
      setError(extractApiError(err));
    } finally {
      setLoading(false);
    }
  };

  return (
    <AuthLayout>
      <AuthHeader
        title="Mot de passe oublié"
        subtitle="Recevez un lien pour choisir un nouveau mot de passe"
      />

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

          <Button type="submit" fullWidth loading={loading} className="mt-6">
            {loading ? 'Veuillez patienter…' : 'Envoyer le lien'}
          </Button>
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
