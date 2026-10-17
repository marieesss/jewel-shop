import { useState, type FormEvent } from 'react';
import { observer } from 'mobx-react-lite';
import { Link, Navigate, useLocation, useNavigate, useSearchParams } from 'react-router-dom';
import { useAuthStore } from '../stores/context';
import { postLoginPath } from '../lib/returnUrl';
import { SESSION_EXPIRED_MESSAGE, isSessionExpiredState } from '../lib/sessionExpired';
import { loginMessageOf } from '../lib/loginMessage';
import { AuthHeader } from '../features/auth/AuthHeader';
import { PasswordField } from '../features/auth/PasswordField';
import { FormInput } from '../components/FormInput';
import { Alert } from '../components/ui/Alert';
import { AuthLayout } from '../features/auth/AuthLayout';
import { MailIcon } from '../components/icons';

export const AuthPage = observer(function AuthPage() {
  const auth = useAuthStore();
  const navigate = useNavigate();
  const [searchParams] = useSearchParams();
  const location = useLocation();
  const destination = () => postLoginPath(searchParams.get('returnUrl'), auth.isAdmin);

  const [email, setEmail] = useState('');
  const [password, setPassword] = useState('');
  const [showPassword, setShowPassword] = useState(false);

  const handleSubmit = async (e: FormEvent) => {
    e.preventDefault();
    auth.clearError();

    const ok = await auth.login({ email, password });
    if (ok) {
      navigate(destination(), { replace: true });
    }
  };

  const sessionExpiredMessage = isSessionExpiredState(location.state)
    ? SESSION_EXPIRED_MESSAGE
    : null;
  const errorMessage = auth.error ?? sessionExpiredMessage;
  const confirmationMessage = loginMessageOf(location.state);

  // Déjà connecté : on ne réaffiche pas le formulaire.
  if (auth.isAuthenticated) {
    return <Navigate to={destination()} replace />;
  }

  return (
    <AuthLayout>
      <AuthHeader title="Bon retour" subtitle="Retrouvez vos créations et favoris" />

      {confirmationMessage && (
        <Alert variant="success" className="mb-[18px]">
          {confirmationMessage}
        </Alert>
      )}

      {/* Carte formulaire */}
      <form
        onSubmit={handleSubmit}
        className="rounded-[20px] border border-white/60 bg-white/70 px-6 py-7 shadow-card backdrop-blur-[20px]"
      >
        <div className="flex flex-col gap-[18px]">
          <FormInput
            label="Email"
            type="email"
            value={email}
            onChange={setEmail}
            placeholder="marie@exemple.com"
            icon={<MailIcon />}
            required
          />

          <PasswordField
            label="Mot de passe"
            value={password}
            onChange={setPassword}
            visible={showPassword}
            onToggleVisible={() => setShowPassword(!showPassword)}
            delay={0.05}
          />
        </div>

        <div className="mt-2 text-right">
          <Link
            to="/forgot-password"
            className="font-body text-xs font-medium text-fuchsia transition-opacity hover:opacity-70"
          >
            Mot de passe oublié ?
          </Link>
        </div>

        {/* Message d'erreur */}
        {errorMessage && (
          <div className="mt-[18px] animate-fade-in rounded-xl border border-poudre bg-blush px-3.5 py-[11px] font-body text-[13px] font-medium text-fuchsia">
            {errorMessage}
          </div>
        )}

        {/* Bouton de soumission */}
        <button
          type="submit"
          disabled={auth.loading}
          className="relative mt-6 w-full overflow-hidden rounded-xl bg-storm py-[14px] font-body text-sm font-medium tracking-[0.04em] text-white shadow-btn transition-all duration-300 hover:-translate-y-0.5 hover:shadow-btn-hover disabled:cursor-wait disabled:opacity-75 disabled:hover:translate-y-0 disabled:hover:shadow-btn"
        >
          {auth.loading ? 'Veuillez patienter…' : 'Se connecter'}
        </button>
      </form>
    </AuthLayout>
  );
});
