import { useState } from 'react';
import type { AdminTag } from '../../api/tags';
import { Button } from '../../components/ui/Button';
import { mergeTargetOptions, tagUsageCount } from '../../lib/tagAdmin';
import { TagMergeForm } from './TagMergeForm';
import { TagRenameForm } from './TagRenameForm';

type RowMode = 'view' | 'rename' | 'merge';

interface TagRowProps {
  tag: AdminTag;
  /** Tous les tags de la page, pour choisir le tag conservé lors d'une fusion. */
  allTags: AdminTag[];
  /** Une action de la page porte sur ce tag : ses boutons sont désactivés. */
  actionInProgress: boolean;
  /** Appelé au début d'une action, pour effacer les messages de l'action précédente. */
  onActionStart: () => void;
  onRenamed: (successMessage: string) => void;
  onDelete: (tag: AdminTag) => void;
  /** Résout à `true` si la fusion a eu lieu. */
  onMerge: (source: AdminTag, target: AdminTag) => Promise<boolean>;
}

/** Ligne d'un tag : nom et nombre d'articles, avec le choix entre renommer, fusionner et supprimer. */
export function TagRow({
  tag,
  allTags,
  actionInProgress,
  onActionStart,
  onRenamed,
  onDelete,
  onMerge,
}: TagRowProps) {
  const [mode, setMode] = useState<RowMode>('view');
  const targetOptions = mergeTargetOptions(tag, allTags);

  const open = (nextMode: RowMode) => {
    onActionStart();
    setMode(nextMode);
  };

  const close = () => setMode('view');

  const handleRenamed = (successMessage: string) => {
    close();
    onRenamed(successMessage);
  };

  const handleMerge = async (target: AdminTag) => {
    const merged = await onMerge(tag, target);
    if (merged) close();
    return merged;
  };

  return (
    <li className="border-b border-mist/60 px-4 py-3 last:border-0">
      {mode === 'rename' ? (
        <TagRenameForm
          tag={tag}
          onActionStart={onActionStart}
          onRenamed={handleRenamed}
          onCancel={close}
        />
      ) : (
        <div className="flex flex-wrap items-center justify-between gap-3">
          <div className="font-body text-sm text-storm">
            <span className="font-medium">{tag.name}</span>
            <span className="ml-2 text-slate">{tagUsageCount(tag)} article(s)</span>
          </div>
          <div className="flex gap-2">
            <Button
              variant="secondary"
              className="px-3 py-1.5"
              disabled={actionInProgress}
              onClick={() => open('rename')}
            >
              Renommer
            </Button>
            <Button
              variant="secondary"
              className="px-3 py-1.5"
              disabled={actionInProgress || targetOptions.length === 0}
              onClick={() => open('merge')}
            >
              Fusionner
            </Button>
            <Button
              variant="ghost"
              className="px-3 py-1.5"
              disabled={actionInProgress}
              onClick={() => onDelete(tag)}
            >
              Supprimer
            </Button>
          </div>
        </div>
      )}

      {mode === 'merge' && (
        <TagMergeForm
          targetOptions={targetOptions}
          allTags={allTags}
          actionInProgress={actionInProgress}
          onMerge={handleMerge}
          onCancel={close}
        />
      )}
    </li>
  );
}
