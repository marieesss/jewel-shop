import { useState, type FormEvent } from 'react';
import { createInvitation } from '../../api/admin';
import { extractApiError } from '../../api/errors';
import { validateInvitationEmail } from '../../lib/invitationEmail';
import { Alert } from '../../components/ui/Alert';
import { Button } from '../../components/ui/Button';
import { Card } from '../../components/ui/Card';
import { TextField } from '../../components/ui/TextField';

interface InvitationFormProps {
  onInvited?: () => void;
}

/** Formulaire d'invitation d'une personne par email (admin). */
export function InvitationForm({ onInvited }: InvitationFormProps) {
  const [email, setEmail] = useState('');
  const [error, setError] = useState<string | null>(null);
  const [success, setSuccess] = useState<string | null>(null);
  const [sending, setSending] = useState(false);

  const handleSubmit = async (e: FormEvent) => {
    e.preventDefault();
    setError(null);
    setSuccess(null);

    // L'API reçoit l'email nettoyé : on valide donc cette même valeur.
    const trimmedEmail = email.trim();
    const [firstEmailError] = validateInvitationEmail(trimmedEmail);
    if (firstEmailError) {
      setError(firstEmailError);
      return;
    }

    setSending(true);
    try {
      await createInvitation(trimmedEmail);
      setSuccess(`Invitation envoyée à ${trimmedEmail}.`);
      setEmail('');
      onInvited?.();
    } catch (err) {
      setError(extractApiError(err));
    } finally {
      setSending(false);
    }
  };

  return (
    <Card className="mb-8">
      <form onSubmit={handleSubmit} noValidate>
        <h2 className="mb-4 font-comfortaa text-xl font-light text-storm">Inviter une personne</h2>

        <TextField
          label="Email"
          type="email"
          value={email}
          onChange={setEmail}
          placeholder="marie@exemple.com"
          required
        />

        {error && <Alert className="mt-4">{error}</Alert>}
        {success && (
          <Alert variant="success" className="mt-4">
            {success}
          </Alert>
        )}

        <Button type="submit" loading={sending} className="mt-5">
          Envoyer l'invitation
        </Button>
      </form>
    </Card>
  );
}
