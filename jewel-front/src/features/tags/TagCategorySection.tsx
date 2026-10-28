import type { AdminTag } from '../../api/tags';
import type { TagCategoryGroup } from '../../lib/tagAdmin';
import { TagRow } from './TagRow';

interface TagCategorySectionProps {
  group: TagCategoryGroup;
  allTags: AdminTag[];
  actionInProgressId: number | null;
  onActionStart: () => void;
  onRenamed: (successMessage: string) => void;
  onDelete: (tag: AdminTag) => void;
  onMerge: (source: AdminTag, target: AdminTag) => Promise<boolean>;
}

/** Tags d'une catégorie (« Thème », « Couleur »…), dans l'ordre renvoyé par l'API. */
export function TagCategorySection({
  group,
  allTags,
  actionInProgressId,
  ...rowActions
}: TagCategorySectionProps) {
  return (
    <section className="mb-6">
      <h2 className="mb-3 font-comfortaa text-lg font-light text-storm">{group.label}</h2>

      {group.tags.length === 0 ? (
        <p className="font-body text-[13px] text-slate">Aucun tag dans cette catégorie.</p>
      ) : (
        <ul className="rounded-[20px] border border-white/60 bg-white shadow-card">
          {group.tags.map((tag) => (
            <TagRow
              key={tag.id}
              tag={tag}
              allTags={allTags}
              actionInProgress={actionInProgressId === tag.id}
              {...rowActions}
            />
          ))}
        </ul>
      )}
    </section>
  );
}
