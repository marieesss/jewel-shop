import { useState, type FormEvent } from 'react';
import { observer } from 'mobx-react-lite';
import { Link, useNavigate, useSearchParams } from 'react-router-dom';
import { getInvitation, type InvitationDetails } from '../api/auth';
import { useAsync } from '../hooks/useAsync';
import { useAuthStore } from '../stores/context';
import { INVALID_INVITATION_MESSAGE, isInvitationNotFoundError } from '../lib/invitation';
import { PASSWORD_MISMATCH_MESSAGE } from '../lib/passwordReset';
import { validatePassword } from '../lib/passwordPolicy';
import { AuthHeader } from '../features/auth/AuthHeader';
import { AuthLayout } from '../features/auth/AuthLayout';
import { PasswordField } from '../features/auth/PasswordField';
import { FormInput } from '../components/FormInput';
import { Alert } from '../components/ui/Alert';
import { Button } from '../components/ui/Button';
import { Spinner } from '../components/ui/Spinner';
import { MailIcon, LockIcon, UserIcon, CalendarIcon } from '../components/icons';

const linkClassName =
  'font-body text-[13px] font-semibold text-fuchsia underline underline-offset-[3px] transition-opacity hover:opacity-70';

/** Renvoie l'invitation en attente, ou null si le token est absent ou ne correspond à aucune invitation valide. */
async function findInvitation(token: string): Promise<InvitationDetails | null> {
  if (token.trim() === '') return null;
  try {
    return await getInvitation(token);
  } catch (err) {
    if (isInvitationNotFoundError(err)) return null;
    throw err;
  }
}

function InvalidInvitation() {
  return (
    <>
      <Alert>{INVALID_INVITATION_MESSAGE}</Alert>
      <p className="mt-6 text-center">
        <Link to="/login" className={linkClassName}>
          Se connecter
        </Link>
      </p>
    </>
  );
}

interface RegisterFormProps {
  token: string;
  invitation: InvitationDetails;
  onInvitationRejected: () => void;
}

const RegisterForm = observer(function RegisterForm({
  token,
  invitation,
  onInvitationRejected,
}: RegisterFormProps) {
  const auth = useAuthStore();
  const navigate = useNavigate();

  const [name, setName] = useState('');
  const [surname, setSurname] = useState('');
  const [password, setPassword] = useState('');
  const [confirmPassword, setConfirmPassword] = useState('');
  const [birthday, setBirthday] = useState('');
  const [showPassword, setShowPassword] = useState(false);
  const [localError, setLocalError] = useState<string | null>(null);

  const handleSubmit = async (e: FormEvent) => {
    e.preventDefault();
    setLocalError(null);
    auth.clearError();

    const [firstPolicyError] = validatePassword(password);
    if (firstPolicyError) {
      setLocalError(firstPolicyError);
      return;
    }
    if (password !== confirmPassword) {
      setLocalError(PASSWORD_MISMATCH_MESSAGE);
      return;
    }

    const outcome = await auth.register({
      name,
      surname,
      email: invitation.email,
      password,
      birthday: birthday || null,
      invitationToken: token,
    });

    if (outcome === 'success') {
      navigate('/', { replace: true });
    } else if (outcome === 'invalid-invitation') {
      onInvitationRejected();
    }
  };

  const errorMessage = localError ?? auth.error;

  return (
    <form
      onSubmit={handleSubmit}
      className="rounded-[20px] border border-white/60 bg-white/70 px-6 py-7 shadow-card backdrop-blur-[20px]"
    >
      <div className="flex flex-col gap-[18px]">
        <FormInput
          label="Email"
          type="email"
          value={invitation.email}
          onChange={() => undefined}
          icon={<MailIcon />}
          readOnly
        />

        <div className="flex gap-3">
          <div className="flex-1">
            <FormInput
              label="Prénom"
              value={name}
              onChange={setName}
              placeholder="Marie"
              icon={<UserIcon />}
              delay={0.05}
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
              delay={0.1}
              required
            />
          </div>
        </div>

        <PasswordField
          label="Mot de passe"
          value={password}
          onChange={setPassword}
          visible={showPassword}
          onToggleVisible={() => setShowPassword(!showPassword)}
          delay={0.15}
          showRules
        />

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
      </div>

      {errorMessage && <Alert className="mt-[18px]">{errorMessage}</Alert>}

      <Button type="submit" fullWidth loading={auth.loading} className="mt-6">
        {auth.loading ? 'Veuillez patienter…' : 'Créer mon compte'}
      </Button>
    </form>
  );
});

export function RegisterPage() {
  const [searchParams] = useSearchParams();
  const token = searchParams.get('token') ?? '';
  const { data: invitation, loading, error } = useAsync(() => findInvitation(token), [token]);
  const [invitationRejected, setInvitationRejected] = useState(false);

  const renderContent = () => {
    if (loading) return <Spinner label="Vérification de l'invitation…" />;
    if (error) return <Alert>{error}</Alert>;
    if (!invitation || invitationRejected) return <InvalidInvitation />;
    return (
      <RegisterForm
        token={token}
        invitation={invitation}
        onInvitationRejected={() => setInvitationRejected(true)}
      />
    );
  };

  return (
    <AuthLayout>
      <AuthHeader title="Rejoignez-nous" subtitle="Commencez à créer vos bijoux uniques" />
      {renderContent()}
    </AuthLayout>
  );
}
