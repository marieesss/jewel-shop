import { useState } from 'react';
import { getUsers } from '../../api/admin';
import { PageHeader } from '../../components/ui/PageHeader';
import { Spinner } from '../../components/ui/Spinner';
import { Alert } from '../../components/ui/Alert';
import { EmptyState } from '../../components/ui/EmptyState';
import { Pagination } from '../../components/ui/Pagination';
import { useAsync } from '../../hooks/useAsync';
import { InvitationForm } from '../../features/users/InvitationForm';
import { InvitationList } from '../../features/users/InvitationList';
import { UserTable } from '../../features/users/UserTable';

const PAGE_SIZE = 20;

/** Gestion des utilisateurs : invitation par email, suivi des invitations et liste paginée des inscrits. */
export function AdminUsersPage() {
  const [page, setPage] = useState(1);
  const [invitationsRefreshKey, setInvitationsRefreshKey] = useState(0);
  const { data, loading, error } = useAsync(() => getUsers(page, PAGE_SIZE), [page]);

  return (
    <div className="mx-auto w-full max-w-[920px]">
      <PageHeader title="Utilisateurs" subtitle="Invitations et comptes inscrits" />

      <InvitationForm onInvited={() => setInvitationsRefreshKey((key) => key + 1)} />

      <InvitationList refreshKey={invitationsRefreshKey} />

      {loading && <Spinner label="Chargement…" />}
      {!loading && error && <Alert>{error}</Alert>}

      {!loading && !error && data && data.items.length === 0 && (
        <EmptyState title="Aucun inscrit" description="Aucun compte n'a encore été créé." />
      )}

      {!loading && !error && data && data.items.length > 0 && (
        <>
          <UserTable users={data.items} />

          <Pagination
            page={data.page}
            totalPages={data.totalPages}
            hasPrevious={data.hasPrevious}
            hasNext={data.hasNext}
            onChange={setPage}
          />
        </>
      )}
    </div>
  );
}
