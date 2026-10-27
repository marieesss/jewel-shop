import { useId, useState, type KeyboardEvent } from 'react';
import {
  createTag,
  getTags,
  TAG_CATEGORY_LABELS,
  type TagCategory,
  type TagDto,
} from '../../api/tags';
import { extractApiError } from '../../api/errors';
import { useAsync } from '../../hooks/useAsync';
import { cn } from '../../lib/cn';
import { Alert } from '../../components/ui/Alert';
import { Button } from '../../components/ui/Button';
import { Checkbox } from '../../components/ui/Checkbox';
import { FieldShell, controlClass } from '../../components/ui/FieldShell';
import { Select } from '../../components/ui/Select';
import { Spinner } from '../../components/ui/Spinner';
import {
  categoriesAvailableFor,
  groupTagsByCategory,
  selectedTags,
  toggleTagId,
  validateTagName,
  withTag,
} from './tagSelection';

interface TagPickerProps {
  selectedIds: number[];
  onChange: (ids: number[]) => void;
  error?: string;
}

/**
 * Sélection multiple des tags d'un produit, groupés par catégorie, avec recherche et création
 * d'un tag à la volée (le tag créé est aussitôt sélectionné).
 */
export function TagPicker({ selectedIds, onChange, error }: TagPickerProps) {
  const searchId = useId();
  const { data, loading, error: loadError } = useAsync(getTags, []);
  const [createdTags, setCreatedTags] = useState<TagDto[]>([]);
  const [search, setSearch] = useState('');
  const [chosenCategory, setChosenCategory] = useState<TagCategory | null>(null);
  const [creating, setCreating] = useState(false);
  const [createError, setCreateError] = useState<string | null>(null);

  const tags = createdTags.reduce(withTag, data ?? []);
  const groups = groupTagsByCategory(tags, search);
  const chosen = selectedTags(tags, selectedIds);
  const creatableCategories = categoriesAvailableFor(search, tags);
  const newTagCategory =
    chosenCategory && creatableCategories.includes(chosenCategory)
      ? chosenCategory
      : creatableCategories[0];
  const newTagName = search.trim();

  const changeSearch = (value: string) => {
    setSearch(value);
    setCreateError(null);
  };

  // Entrée dans la recherche ne doit pas enregistrer tout le formulaire du produit.
  const preventFormSubmit = (e: KeyboardEvent<HTMLInputElement>) => {
    if (e.key === 'Enter') e.preventDefault();
  };

  const handleCreate = async () => {
    if (!newTagCategory) return;
    const nameError = validateTagName(newTagName);
    if (nameError) {
      setCreateError(nameError);
      return;
    }

    setCreating(true);
    setCreateError(null);
    try {
      const tag = await createTag({ name: newTagName, category: newTagCategory });
      setCreatedTags((created) => withTag(created, tag));
      if (!selectedIds.includes(tag.id)) onChange([...selectedIds, tag.id]);
      setSearch('');
    } catch (err) {
      setCreateError(extractApiError(err));
    } finally {
      setCreating(false);
    }
  };

  return (
    <FieldShell label="Tags" htmlFor={searchId} error={error}>
      <div className="mb-2 flex flex-wrap gap-1.5">
        {chosen.length === 0 && (
          <p className="font-body text-[13px] text-slate/80">Aucun tag sélectionné.</p>
        )}
        {chosen.map((tag) => (
          <button
            key={tag.id}
            type="button"
            onClick={() => onChange(toggleTagId(selectedIds, tag.id))}
            aria-label={`Retirer le tag ${tag.name}`}
            className="inline-flex items-center gap-1 rounded-full bg-blush px-2.5 py-1 font-body text-[12px] font-medium text-fuchsia transition-opacity hover:opacity-70"
          >
            {tag.name}
            <span aria-hidden="true">×</span>
          </button>
        ))}
      </div>

      <input
        id={searchId}
        type="search"
        value={search}
        onChange={(e) => changeSearch(e.target.value)}
        onKeyDown={preventFormSubmit}
        placeholder="Rechercher ou créer un tag…"
        className={cn(controlClass, error && 'border-fuchsia')}
      />

      <div className="mt-2 max-h-60 overflow-y-auto rounded-xl border border-lin px-4 py-3">
        {loading && <Spinner className="py-4" />}
        {!loading && loadError && <Alert>{loadError}</Alert>}
        {!loading && !loadError && groups.length === 0 && (
          <p className="font-body text-[13px] text-slate/80">Aucun tag ne correspond.</p>
        )}
        {groups.map((group) => (
          <div key={group.category} className="mb-3 last:mb-0">
            <p className="mb-1.5 font-body text-[11px] font-medium uppercase tracking-[0.08em] text-slate">
              {group.label}
            </p>
            <div className="flex flex-col gap-1.5">
              {group.tags.map((tag) => (
                <Checkbox
                  key={tag.id}
                  label={tag.name}
                  checked={selectedIds.includes(tag.id)}
                  onChange={() => onChange(toggleTagId(selectedIds, tag.id))}
                />
              ))}
            </div>
          </div>
        ))}
      </div>

      {!loading && !loadError && newTagCategory && (
        <div className="mt-3 flex flex-col gap-2 rounded-xl bg-lin/50 p-3">
          <Select
            label="Catégorie du nouveau tag"
            value={newTagCategory}
            onChange={(value) => setChosenCategory(value as TagCategory)}
            options={creatableCategories.map((category) => ({
              value: category,
              label: TAG_CATEGORY_LABELS[category],
            }))}
            required
          />
          <Button
            type="button"
            variant="secondary"
            loading={creating}
            onClick={handleCreate}
            className="py-2"
          >
            {`Créer « ${newTagName} »`}
          </Button>
          {createError && (
            <p className="font-body text-[11px] font-medium text-fuchsia">{createError}</p>
          )}
        </div>
      )}
    </FieldShell>
  );
}
