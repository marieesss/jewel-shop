import { useState } from 'react';
import { deleteChainLength, getAdminChainLengths, type ChainLength } from '../../api/chainLengths';
import { extractApiError } from '../../api/errors';
import { Alert } from '../../components/ui/Alert';
import { EmptyState } from '../../components/ui/EmptyState';
import { PageHeader } from '../../components/ui/PageHeader';
import { Spinner } from '../../components/ui/Spinner';
import { useAsync } from '../../hooks/useAsync';
import { ChainLengthForm } from '../../features/chainLengths/ChainLengthForm';
import { ChainLengthTable } from '../../features/chainLengths/ChainLengthTable';
import { DELETE_SUCCESS_MESSAGE, deleteConfirmationMessage } from '../../lib/chainLengthForm';

/** Longueurs de chaîne proposées au client : liste, ajout, modification et suppression. */
export function AdminChainLengthsPage() {
  const { data, loading, error, reload } = useAsync(getAdminChainLengths, []);
  const [editing, setEditing] = useState<ChainLength | null>(null);
  const [actionError, setActionError] = useState<string | null>(null);
  const [actionSuccess, setActionSuccess] = useState<string | null>(null);
  const [actionInProgressId, setActionInProgressId] = useState<number | null>(null);

  const clearActionMessages = () => {
    setActionError(null);
    setActionSuccess(null);
  };

  const handleSaved = (successMessage: string) => {
    setEditing(null);
    setActionError(null);
    setActionSuccess(successMessage);
    reload();
  };

  const handleEdit = (length: ChainLength) => {
    clearActionMessages();
    setEditing(length);
    window.scrollTo({ top: 0, behavior: 'smooth' });
  };

  const handleCancel = () => {
    clearActionMessages();
    setEditing(null);
  };

  const handleDelete = async (length: ChainLength) => {
    if (!window.confirm(deleteConfirmationMessage(length))) return;

    clearActionMessages();
    setActionInProgressId(length.id);
    try {
      await deleteChainLength(length.id);
      if (editing?.id === length.id) setEditing(null);
      setActionSuccess(DELETE_SUCCESS_MESSAGE);
    } catch (err) {
      setActionError(extractApiError(err));
    } finally {
      setActionInProgressId(null);
      reload();
    }
  };

  return (
    <div className="mx-auto w-full max-w-[920px]">
      <PageHeader title="Longueurs" subtitle="Longueurs de chaîne proposées au client" />

      <ChainLengthForm
        key={editing?.id ?? 'new'}
        length={editing ?? undefined}
        onSubmitStart={clearActionMessages}
        onSaved={handleSaved}
        onCancel={handleCancel}
      />

      {actionError && <Alert className="mb-4">{actionError}</Alert>}
      {actionSuccess && (
        <Alert variant="success" className="mb-4">
          {actionSuccess}
        </Alert>
      )}

      {loading && <Spinner label="Chargement…" />}
      {!loading && error && <Alert>{error}</Alert>}

      {!loading && !error && data && data.length === 0 && <EmptyState title="Aucune longueur." />}

      {!loading && !error && data && data.length > 0 && (
        <ChainLengthTable
          lengths={data}
          actionInProgressId={actionInProgressId}
          onEdit={handleEdit}
          onDelete={handleDelete}
        />
      )}
    </div>
  );
}
