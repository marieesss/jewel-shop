import { useState, type FormEvent } from 'react';
import type { AdminTag } from '../../api/tags';
import { Button } from '../../components/ui/Button';
import { Select, type SelectOption } from '../../components/ui/Select';

interface TagMergeFormProps {
  /** Tags proposés comme tag conservé (jamais vide : le bouton « Fusionner » de la ligne est alors désactivé). */
  targetOptions: SelectOption[];
  allTags: AdminTag[];
  /** Une action de la page porte sur ce tag. */
  actionInProgress: boolean;
  /** Résout à `true` si la fusion a eu lieu. */
  onMerge: (target: AdminTag) => Promise<boolean>;
  onCancel: () => void;
}

/** Choix du tag conservé lors d'une fusion, puis confirmation. */
export function TagMergeForm({
  targetOptions,
  allTags,
  actionInProgress,
  onMerge,
  onCancel,
}: TagMergeFormProps) {
  const [targetId, setTargetId] = useState(targetOptions[0]?.value ?? '');
  const [saving, setSaving] = useState(false);

  const handleSubmit = async (e: FormEvent) => {
    e.preventDefault();
    const target = allTags.find((candidate) => String(candidate.id) === targetId);
    if (!target) return;

    setSaving(true);
    try {
      await onMerge(target);
    } finally {
      setSaving(false);
    }
  };

  return (
    <form onSubmit={handleSubmit} noValidate className="mt-3 flex flex-wrap items-end gap-3">
      <div className="min-w-[200px] flex-1">
        <Select
          label="Tag conservé"
          value={targetId}
          onChange={setTargetId}
          options={targetOptions}
          required
        />
      </div>
      <Button type="submit" className="px-3 py-1.5" loading={saving} disabled={actionInProgress}>
        Fusionner
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
