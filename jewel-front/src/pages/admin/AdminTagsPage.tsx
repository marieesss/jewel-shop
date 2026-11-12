import { useState } from 'react';
import { extractApiError } from '../../api/errors';
import { deleteTag, getAdminTags, mergeTag, type AdminTag } from '../../api/tags';
import { Alert } from '../../components/ui/Alert';
import { PageHeader } from '../../components/ui/PageHeader';
import { Spinner } from '../../components/ui/Spinner';
import { TagCategorySection } from '../../features/tags/TagCategorySection';
import { TagCreateForm } from '../../features/tags/TagCreateForm';
import { useAsync } from '../../hooks/useAsync';
import {
  DELETE_SUCCESS_MESSAGE,
  MERGE_SUCCESS_MESSAGE,
  deleteConfirmationMessage,
  groupTagsByCategory,
  mergeConfirmationMessage,
} from '../../lib/tagAdmin';

/** Tags du catalogue, groupés par catégorie : création, renommage, fusion et suppression. */
export function AdminTagsPage() {
  const { data, loading, error, reload } = useAsync(getAdminTags, []);
  const [actionError, setActionError] = useState<string | null>(null);
  const [actionSuccess, setActionSuccess] = useState<string | null>(null);
  const [actionInProgressId, setActionInProgressId] = useState<number | null>(null);

  const clearActionMessages = () => {
    setActionError(null);
    setActionSuccess(null);
  };

  const handleSaved = (successMessage: string) => {
    setActionError(null);
    setActionSuccess(successMessage);
    reload();
  };

  const handleDelete = async (tag: AdminTag) => {
    if (!window.confirm(deleteConfirmationMessage(tag))) return;

    clearActionMessages();
    setActionInProgressId(tag.id);
    try {
      await deleteTag(tag.id);
      setActionSuccess(DELETE_SUCCESS_MESSAGE);
    } catch (err) {
      setActionError(extractApiError(err));
    } finally {
      setActionInProgressId(null);
      reload();
    }
  };

  const handleMerge = async (source: AdminTag, target: AdminTag): Promise<boolean> => {
    if (!window.confirm(mergeConfirmationMessage(source, target))) return false;

    clearActionMessages();
    setActionInProgressId(source.id);
    try {
      await mergeTag(source.id, target.id);
      setActionSuccess(MERGE_SUCCESS_MESSAGE);
      return true;
    } catch (err) {
      setActionError(extractApiError(err));
      return false;
    } finally {
      setActionInProgressId(null);
      reload();
    }
  };

  return (
    <div className="mx-auto w-full max-w-[920px]">
      <PageHeader title="Tags" subtitle="Thèmes, couleurs, matières et occasions du catalogue" />

      <TagCreateForm onSubmitStart={clearActionMessages} onCreated={handleSaved} />

      {actionError && <Alert className="mb-4">{actionError}</Alert>}
      {actionSuccess && (
        <Alert variant="success" className="mb-4">
          {actionSuccess}
        </Alert>
      )}

      {loading && <Spinner label="Chargement…" />}
      {!loading && error && <Alert>{error}</Alert>}

      {!loading &&
        !error &&
        data &&
        groupTagsByCategory(data).map((group) => (
          <TagCategorySection
            key={group.category}
            group={group}
            allTags={data}
            actionInProgressId={actionInProgressId}
            onActionStart={clearActionMessages}
            onRenamed={handleSaved}
            onDelete={handleDelete}
            onMerge={handleMerge}
          />
        ))}
    </div>
  );
}
