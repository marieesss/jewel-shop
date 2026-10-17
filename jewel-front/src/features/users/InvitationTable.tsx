import type { Invitation } from '../../api/admin';
import { Button } from '../../components/ui/Button';
import { formatDate } from '../../lib/format';
import { canResendInvitation, canRevokeInvitation } from '../../lib/invitationActions';

const UNKNOWN_INVITER = '—';

const COLUMNS = ['Email', 'Invitée par', 'Envoyée le', 'Expire le', 'Actions'];

interface InvitationTableProps {
  invitations: Invitation[];
  /** Invitation dont une action est en cours : ses boutons sont désactivés. */
  actionInProgressId: number | null;
  onResend: (invitation: Invitation) => void;
  onRevoke: (invitation: Invitation) => void;
}

/** Tableau des invitations (admin), avec les actions permises par leur statut. */
export function InvitationTable({
  invitations,
  actionInProgressId,
  onResend,
  onRevoke,
}: InvitationTableProps) {
  return (
    <div className="overflow-x-auto rounded-[20px] border border-white/60 bg-white shadow-card">
      <table className="w-full font-body text-sm text-storm">
        <thead>
          <tr className="border-b border-mist text-left text-xs uppercase tracking-wide text-slate">
            {COLUMNS.map((column) => (
              <th key={column} scope="col" className="whitespace-nowrap px-4 py-3 font-semibold">
                {column}
              </th>
            ))}
          </tr>
        </thead>
        <tbody>
          {invitations.map((invitation) => {
            const isActionInProgress = actionInProgressId === invitation.id;
            return (
              <tr key={invitation.id} className="border-b border-mist/60 last:border-0">
                <td className="px-4 py-3">{invitation.email}</td>
                <td className="px-4 py-3">{invitation.invitedBy ?? UNKNOWN_INVITER}</td>
                <td className="whitespace-nowrap px-4 py-3">{formatDate(invitation.createdAt)}</td>
                <td className="whitespace-nowrap px-4 py-3">{formatDate(invitation.expiresAt)}</td>
                <td className="whitespace-nowrap px-4 py-3">
                  <div className="flex gap-2">
                    {canResendInvitation(invitation.status) && (
                      <Button
                        variant="secondary"
                        className="px-3 py-1.5"
                        disabled={isActionInProgress}
                        onClick={() => onResend(invitation)}
                      >
                        Renvoyer
                      </Button>
                    )}
                    {canRevokeInvitation(invitation.status) && (
                      <Button
                        variant="ghost"
                        className="px-3 py-1.5"
                        disabled={isActionInProgress}
                        onClick={() => onRevoke(invitation)}
                      >
                        Révoquer
                      </Button>
                    )}
                  </div>
                </td>
              </tr>
            );
          })}
        </tbody>
      </table>
    </div>
  );
}
