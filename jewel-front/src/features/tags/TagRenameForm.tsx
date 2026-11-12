import { useState, type FormEvent } from 'react';
import { renameTag, type AdminTag } from '../../api/tags';
import { Button } from '../../components/ui/Button';
import { TextField } from '../../components/ui/TextField';
import {
  RENAME_SUCCESS_MESSAGE,
  TAG_NAME_MAX_LENGTH,
  noTagFormErrors,
  tagFormErrorsFromApi,
  tagNameFormErrors,
  type TagFormErrors,
} from '../../lib/tagAdmin';

interface TagRenameFormProps {
  tag: AdminTag;
  /** Appelé au début d'un envoi, pour effacer les messages de l'action précédente. */
  onActionStart: () => void;
  onRenamed: (successMessage: string) => void;
  onCancel: () => void;
}

/** Renommage en ligne d'un tag. */
export function TagRenameForm({ tag, onActionStart, onRenamed, onCancel }: TagRenameFormProps) {
  const [name, setName] = useState(tag.name);
  const [errors, setErrors] = useState<TagFormErrors>(noTagFormErrors);
  const [saving, setSaving] = useState(false);

  const handleSubmit = async (e: FormEvent) => {
    e.preventDefault();
    onActionStart();

    const nameErrors = tagNameFormErrors(name);
    setErrors(nameErrors);
    if (nameErrors.fields.name) return;

    setSaving(true);
    try {
      await renameTag(tag.id, name.trim());
      onRenamed(RENAME_SUCCESS_MESSAGE);
    } catch (err) {
      setErrors(tagFormErrorsFromApi(err));
      setSaving(false);
    }
  };

  return (
    <form onSubmit={handleSubmit} noValidate className="flex flex-wrap items-end gap-3">
      <div className="min-w-[200px] flex-1">
        <TextField
          label="Nouveau nom"
          maxLength={TAG_NAME_MAX_LENGTH}
          value={name}
          onChange={setName}
          error={errors.fields.name ?? errors.form ?? undefined}
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
        onClick={onCancel}
      >
        Annuler
      </Button>
    </form>
  );
}
