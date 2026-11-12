import { getTagCounts, type TagCountKind } from '../../api/tags';
import { useAsync } from '../../hooks/useAsync';
import { Alert } from '../../components/ui/Alert';
import { Button } from '../../components/ui/Button';
import { Checkbox } from '../../components/ui/Checkbox';
import { Spinner } from '../../components/ui/Spinner';
import {
  TAGS_LOAD_ERROR_MESSAGE,
  tagFilterLabel,
  toggleTagSlug,
  visibleTagGroups,
  type VisibleTagGroup,
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

  const groups = error ? [] : visibleTagGroups(data ?? [], selectedSlugs);
  // Le bouton reste proposé même sans tag affiché (slug inconnu, échec du chargement)
  // pour pouvoir vider l'adresse.
  const hasSelection = selectedSlugs.length > 0;
  if (!error && groups.length === 0 && !hasSelection) return null;

  return (
    <div className="flex flex-col gap-4">
      {error && <Alert>{TAGS_LOAD_ERROR_MESSAGE}</Alert>}
      {groups.map((group) => (
        <TagFilterGroup
          key={group.category}
          group={group}
          selectedSlugs={selectedSlugs}
          onChange={onChange}
        />
      ))}
      {hasSelection && (
        <Button variant="ghost" className="justify-start px-0 py-1" onClick={() => onChange([])}>
          Effacer les filtres
        </Button>
      )}
    </div>
  );
}

interface TagFilterGroupProps {
  group: VisibleTagGroup;
  selectedSlugs: string[];
  onChange: (slugs: string[]) => void;
}

function TagFilterGroup({ group, selectedSlugs, onChange }: TagFilterGroupProps) {
  return (
    <div>
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
  );
}
