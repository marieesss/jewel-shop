import { getTagCounts, type TagCountKind } from '../../api/tags';
import { useAsync } from '../../hooks/useAsync';
import { Alert } from '../../components/ui/Alert';
import { Checkbox } from '../../components/ui/Checkbox';
import { Spinner } from '../../components/ui/Spinner';
import {
  TAGS_LOAD_ERROR_MESSAGE,
  tagFilterLabel,
  toggleTagSlug,
  visibleTagGroups,
} from './tagFiltering';

interface TagFiltersProps {
  /** Type d'article dont on affiche les tags et les nombres. */
  kind: TagCountKind;
  /** Slugs cochés. */
  selectedSlugs: string[];
  /** Nouvelle liste de slugs cochés (vide pour « Effacer les filtres »). */
  onChange: (slugs: string[]) => void;
}

/**
 * Filtres par tags d'une liste d'articles, groupés par catégorie, avec le nombre d'articles de
 * chaque tag. Ne lit ni n'écrit l'adresse : la page lui passe la sélection.
 */
export function TagFilters({ kind, selectedSlugs, onChange }: TagFiltersProps) {
  const { data, loading, error } = useAsync(() => getTagCounts(kind), [kind]);

  if (loading) return <Spinner className="py-4" label="Chargement des tags…" />;
  if (error) return <Alert>{TAGS_LOAD_ERROR_MESSAGE}</Alert>;

  const groups = visibleTagGroups(data ?? []);
  if (groups.length === 0) return null;

  return (
    <div className="flex flex-col gap-4">
      {groups.map((group) => (
        <div key={group.category}>
          <p className="mb-1.5 font-body text-[11px] font-medium uppercase tracking-[0.08em] text-slate">
            {group.label}
          </p>
          <div className="flex flex-col gap-1.5">
            {group.tags.map((tag) => (
              <Checkbox
                key={tag.id}
                label={tagFilterLabel(tag)}
                checked={selectedSlugs.includes(tag.slug)}
                onChange={() => onChange(toggleTagSlug(selectedSlugs, tag.slug))}
              />
            ))}
          </div>
        </div>
      ))}
    </div>
  );
}
