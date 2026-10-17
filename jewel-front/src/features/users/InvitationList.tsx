import { useEffect, useState } from 'react';
import {
  getInvitations,
  resendInvitation,
  revokeInvitation,
  type Invitation,
  type InvitationStatus,
} from '../../api/admin';
import { extractApiError } from '../../api/errors';
import { Alert } from '../../components/ui/Alert';
import { EmptyState } from '../../components/ui/EmptyState';
import { Pagination } from '../../components/ui/Pagination';
import { Select } from '../../components/ui/Select';
import { Spinner } from '../../components/ui/Spinner';
import { useAsync } from '../../hooks/useAsync';
import {
  DEFAULT_INVITATION_STATUS,
  INVITATION_STATUS_OPTIONS,
  REVOKE_SUCCESS_MESSAGE,
  resendSuccessMessage,
  revokeConfirmationMessage,
} from '../../lib/invitationActions';
import { InvitationTable } from './InvitationTable';

const PAGE_SIZE = 20;

interface InvitationListProps {
  /** Change quand une invitation est créée ailleurs sur la page : la liste est alors rechargée. */
  refreshKey: number;
}

/** Invitations filtrées par statut, avec renvoi et révocation (admin). */
export function InvitationList({ refreshKey }: InvitationListProps) {
  const [status, setStatus] = useState<InvitationStatus>(DEFAULT_INVITATION_STATUS);
  const [page, setPage] = useState(1);
  const [actionError, setActionError] = useState<string | null>(null);
  const [actionSuccess, setActionSuccess] = useState<string | null>(null);
  const [actionInProgressId, setActionInProgressId] = useState<number | null>(null);
  const { data, loading, error, reload } = useAsync(
    () => getInvitations(status, page, PAGE_SIZE),
    [status, page, refreshKey]
  );

  // Une action peut retirer la dernière invitation de la dernière page : on revient alors à la page précédente.
  const isPastLastPage = !loading && !error && data !== null && data.items.length === 0 && page > 1;
  useEffect(() => {
    if (isPastLastPage) setPage((current) => current - 1);
  }, [isPastLastPage]);

  const clearActionMessages = () => {
    setActionError(null);
    setActionSuccess(null);
  };

  const handleStatusChange = (value: string) => {
    setStatus(value as InvitationStatus);
    setPage(1);
    clearActionMessages();
  };

  const runAction = async (
    invitation: Invitation,
    action: () => Promise<unknown>,
    successMessage: string
  ) => {
    clearActionMessages();
    setActionInProgressId(invitation.id);
    try {
      await action();
      setActionSuccess(successMessage);
      reload();
    } catch (err) {
      setActionError(extractApiError(err));
    } finally {
      setActionInProgressId(null);
    }
  };

  const handleResend = (invitation: Invitation) =>
    runAction(
      invitation,
      () => resendInvitation(invitation.id),
      resendSuccessMessage(invitation.email)
    );

  const handleRevoke = (invitation: Invitation) => {
    if (!window.confirm(revokeConfirmationMessage(invitation.email))) return;
    runAction(invitation, () => revokeInvitation(invitation.id), REVOKE_SUCCESS_MESSAGE);
  };

  return (
    <section className="mb-8">
      <div className="mb-4 flex flex-wrap items-end justify-between gap-4">
        <h2 className="font-comfortaa text-xl font-light text-storm">Invitations</h2>
        <div className="w-full sm:w-56">
          <Select
            label="Statut"
            value={status}
            onChange={handleStatusChange}
            options={INVITATION_STATUS_OPTIONS}
          />
        </div>
      </div>

      {actionError && <Alert className="mb-4">{actionError}</Alert>}
      {actionSuccess && (
        <Alert variant="success" className="mb-4">
          {actionSuccess}
        </Alert>
      )}

      {loading && <Spinner label="Chargement…" />}
      {!loading && error && <Alert>{error}</Alert>}

      {!loading && !error && data && data.items.length === 0 && (
        <EmptyState title="Aucune invitation." />
      )}

      {!loading && !error && data && data.items.length > 0 && (
        <>
          <InvitationTable
            invitations={data.items}
            actionInProgressId={actionInProgressId}
            onResend={handleResend}
            onRevoke={handleRevoke}
          />

          <Pagination
            page={data.page}
            totalPages={data.totalPages}
            hasPrevious={data.hasPrevious}
            hasNext={data.hasNext}
            onChange={setPage}
          />
        </>
      )}
    </section>
  );
}
