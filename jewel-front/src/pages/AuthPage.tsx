import { useState, type FormEvent } from 'react';
import { observer } from 'mobx-react-lite';
import { Navigate, useNavigate } from 'react-router-dom';
import { useAuthStore } from '../stores/context';
import { FormInput } from '../components/FormInput';
import { DecoPanel } from '../components/DecoPanel';
import { MailIcon, LockIcon, UserIcon, CalendarIcon } from '../components/icons';

type Mode = 'login' | 'register';

export const AuthPage = observer(function AuthPage() {
  const auth = useAuthStore();
  const navigate = useNavigate();

  const [mode, setMode] = useState<Mode>('login');
  const [animKey, setAnimKey] = useState(0);

  const [email, setEmail] = useState('');
  const [password, setPassword] = useState('');
  const [name, setName] = useState('');
  const [surname, setSurname] = useState('');
  const [birthday, setBirthday] = useState('');
  const [confirmPassword, setConfirmPassword] = useState('');
  const [showPassword, setShowPassword] = useState(false);
  const [localError, setLocalError] = useState<string | null>(null);

  const switchMode = (newMode: Mode) => {
    setMode(newMode);
    setAnimKey((k) => k + 1);
    setLocalError(null);
    auth.clearError();
  };

  const handleSubmit = async (e: FormEvent) => {
    e.preventDefault();
    setLocalError(null);
    auth.clearError();

    let ok = false;
    if (mode === 'register') {
      if (password !== confirmPassword) {
        setLocalError('Les mots de passe ne correspondent pas.');
        return;
      }
      if (password.length < 8) {
        setLocalError('Le mot de passe doit contenir au moins 8 caractères.');
        return;
      }
      ok = await auth.register({
        name,
        surname,
        email,
        password,
        birthday: birthday || null,
      });
    } else {
      ok = await auth.login({ email, password });
    }

    if (ok) {
      navigate(auth.isAdmin ? '/admin' : '/', { replace: true });
    }
  };

  const errorMessage = localError ?? auth.error;

  // Déjà connecté : on ne réaffiche pas le formulaire.
  if (auth.isAuthenticated) {
    return <Navigate to={auth.isAdmin ? '/admin' : '/'} replace />;
  }

  return (
    <div className="flex h-screen w-screen overflow-hidden bg-creme">
      {/* Panneau décoratif gauche */}
      <div className="flex h-full w-[42%] min-w-[380px] shrink-0 animate-[fade-in_0.6s_ease-out]">
        <DecoPanel />
      </div>

      {/* Panneau formulaire droite */}
      <div className="relative flex flex-1 items-start justify-center overflow-auto p-8">
        <div
          key={animKey}
          className="my-auto w-full max-w-[420px] animate-[slide-up_0.5s_ease-out]"
        >
          {/* Bascule Connexion / Inscription */}
          <div className="mb-8 flex gap-1 rounded-[14px] bg-lin p-1">
            {(['login', 'register'] as Mode[]).map((m) => (
              <button
                key={m}
                type="button"
                onClick={() => switchMode(m)}
                className={`flex-1 rounded-[11px] py-[11px] font-body text-[13px] font-medium tracking-[0.03em] transition-all duration-300 ${
                  mode === m
                    ? 'bg-white text-fuchsia shadow-toggle'
                    : 'bg-transparent text-slate'
                }`}
              >
                {m === 'login' ? 'Connexion' : 'Inscription'}
              </button>
            ))}
          </div>

          {/* En-tête */}
          <div className="mb-7">
            <h2 className="mb-1.5 font-comfortaa text-[36px] font-light leading-[1.15] text-storm">
              {mode === 'login' ? 'Bon retour' : 'Rejoignez-nous'}
            </h2>
            <p className="font-display text-base italic text-slate">
              {mode === 'login'
                ? 'Retrouvez vos créations et favoris'
                : 'Commencez à créer vos bijoux uniques'}
            </p>
          </div>

          {/* Carte formulaire */}
          <form
            onSubmit={handleSubmit}
            className="rounded-[20px] border border-white/60 bg-white/70 px-6 py-7 shadow-card backdrop-blur-[20px]"
          >
            <div className="flex flex-col gap-[18px]">
              {mode === 'register' && (
                <div className="flex gap-3">
                  <div className="flex-1">
                    <FormInput
                      label="Prénom"
                      value={name}
                      onChange={setName}
                      placeholder="Marie"
                      icon={<UserIcon />}
                      delay={0}
                      required
                    />
                  </div>
                  <div className="flex-1">
                    <FormInput
                      label="Nom"
                      value={surname}
                      onChange={setSurname}
                      placeholder="Dupont"
                      icon={<UserIcon />}
                      delay={0.05}
                      required
                    />
                  </div>
                </div>
              )}

              <FormInput
                label="Email"
                type="email"
                value={email}
                onChange={setEmail}
                placeholder="marie@exemple.com"
                icon={<MailIcon />}
                delay={mode === 'register' ? 0.1 : 0}
                required
              />

              <div>
                <FormInput
                  label="Mot de passe"
                  type={showPassword ? 'text' : 'password'}
                  value={password}
                  onChange={setPassword}
                  placeholder="••••••••"
                  icon={<LockIcon />}
                  delay={mode === 'register' ? 0.15 : 0.05}
                  required
                />
                <button
                  type="button"
                  onClick={() => setShowPassword(!showPassword)}
                  className="mt-1 py-0.5 font-body text-[11px] text-slate transition-colors hover:text-fuchsia"
                >
                  {showPassword ? 'Masquer' : 'Afficher'}
                </button>
              </div>

              {mode === 'register' && (
                <>
                  <FormInput
                    label="Confirmer le mot de passe"
                    type={showPassword ? 'text' : 'password'}
                    value={confirmPassword}
                    onChange={setConfirmPassword}
                    placeholder="••••••••"
                    icon={<LockIcon />}
                    delay={0.2}
                    required
                  />
                  <FormInput
                    label="Date de naissance (optionnel)"
                    type="date"
                    value={birthday}
                    onChange={setBirthday}
                    icon={<CalendarIcon />}
                    delay={0.25}
                  />
                </>
              )}
            </div>

            {mode === 'login' && (
              <div className="mt-2 text-right">
                <button
                  type="button"
                  className="font-body text-xs font-medium text-fuchsia transition-opacity hover:opacity-70"
                >
                  Mot de passe oublié ?
                </button>
              </div>
            )}

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
              {auth.loading
                ? 'Veuillez patienter…'
                : mode === 'login'
                  ? 'Se connecter'
                  : 'Créer mon compte'}
            </button>
          </form>

          {/* Lien bas de page */}
          <p className="mt-6 text-center font-body text-[13px] text-slate">
            {mode === 'login' ? 'Pas encore de compte ?' : 'Déjà un compte ?'}{' '}
            <button
              type="button"
              onClick={() => switchMode(mode === 'login' ? 'register' : 'login')}
              className="font-body text-[13px] font-semibold text-fuchsia underline underline-offset-[3px] transition-opacity hover:opacity-70"
            >
              {mode === 'login' ? "S'inscrire" : 'Se connecter'}
            </button>
          </p>
        </div>
      </div>
    </div>
  );
});
