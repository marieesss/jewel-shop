import { useState, type FormEvent } from 'react';
import { renameTag, type AdminTag } from '../../api/tags';
import { Button } from '../../components/ui/Button';
import { Select } from '../../components/ui/Select';
import { TextField } from '../../components/ui/TextField';
import {
  RENAME_SUCCESS_MESSAGE,
  TAG_NAME_MAX_LENGTH,
  mergeTargetOptions,
  noTagFormErrors,
  tagFormErrorsFromApi,
  tagUsageCount,
  validateTagName,
  type TagFormErrors,
} from '../../lib/tagAdmin';

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

/** Ligne d'un tag : nom et nombre d'articles, renommage en ligne, fusion et suppression. */
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
  const [name, setName] = useState(tag.name);
  const [errors, setErrors] = useState<TagFormErrors>(noTagFormErrors);
  const [saving, setSaving] = useState(false);
  const targetOptions = mergeTargetOptions(tag, allTags);
  const [targetId, setTargetId] = useState('');

  const open = (nextMode: RowMode) => {
    onActionStart();
    setErrors(noTagFormErrors);
    setName(tag.name);
    setTargetId(targetOptions[0]?.value ?? '');
    setMode(nextMode);
  };

  const close = () => {
    setErrors(noTagFormErrors);
    setMode('view');
  };

  const handleRename = async (e: FormEvent) => {
    e.preventDefault();
    onActionStart();

    const nameError = validateTagName(name);
    setErrors({ fields: nameError ? { name: nameError } : {}, form: null });
    if (nameError) return;

    setSaving(true);
    try {
      await renameTag(tag.id, name.trim());
      setMode('view');
      onRenamed(RENAME_SUCCESS_MESSAGE);
    } catch (err) {
      setErrors(tagFormErrorsFromApi(err));
    } finally {
      setSaving(false);
    }
  };

  const handleMerge = async (e: FormEvent) => {
    e.preventDefault();
    const target = allTags.find((candidate) => String(candidate.id) === targetId);
    if (!target) return;

    setSaving(true);
    try {
      if (await onMerge(tag, target)) setMode('view');
    } finally {
      setSaving(false);
    }
  };

  const disabled = actionInProgress || saving;
  const renameError = errors.fields.name ?? errors.form ?? undefined;

  return (
    <li className="border-b border-mist/60 px-4 py-3 last:border-0">
      {mode === 'rename' ? (
        <form onSubmit={handleRename} noValidate className="flex flex-wrap items-end gap-3">
          <div className="min-w-[200px] flex-1">
            <TextField
              label="Nouveau nom"
              maxLength={TAG_NAME_MAX_LENGTH}
              value={name}
              onChange={setName}
              error={renameError}
              autoFocus
              required
            />
          </div>
          <Button type="submit" className="px-3 py-1.5" loading={saving}>
            Enregistrer
          </Button>
          <Button
            type="button"
            variant="ghost"
            className="px-3 py-1.5"
            disabled={saving}
            onClick={close}
          >
            Annuler
          </Button>
        </form>
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
              disabled={disabled}
              onClick={() => open('rename')}
            >
              Renommer
            </Button>
            <Button
              variant="secondary"
              className="px-3 py-1.5"
              disabled={disabled || targetOptions.length === 0}
              onClick={() => open('merge')}
            >
              Fusionner
            </Button>
            <Button
              variant="ghost"
              className="px-3 py-1.5"
              disabled={disabled}
              onClick={() => onDelete(tag)}
            >
              Supprimer
            </Button>
          </div>
        </div>
      )}

      {mode === 'merge' && (
        <form onSubmit={handleMerge} noValidate className="mt-3 flex flex-wrap items-end gap-3">
          <div className="min-w-[200px] flex-1">
            <Select
              label="Tag conservé"
              value={targetId}
              onChange={setTargetId}
              options={targetOptions}
              required
            />
          </div>
          <Button
            type="submit"
            className="px-3 py-1.5"
            loading={saving}
            disabled={actionInProgress}
          >
            Fusionner
          </Button>
          <Button
            type="button"
            variant="ghost"
            className="px-3 py-1.5"
            disabled={saving}
            onClick={close}
          >
            Annuler
          </Button>
        </form>
      )}
    </li>
  );
}
