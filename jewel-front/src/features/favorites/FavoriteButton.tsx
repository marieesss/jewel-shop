import { observer } from 'mobx-react-lite';
import { useFavoritesStore } from '../../stores/context';
import { HeartIcon } from '../../components/icons-admin';
import { cn } from '../../lib/cn';
import type { ProductKind } from '../../stores/ProductAdminStore';

interface FavoriteButtonProps {
  kind: ProductKind;
  productId: number;
  /** « overlay » : pastille flottante sur une vignette ; « inline » : bouton dans une page. */
  variant?: 'overlay' | 'inline';
}

/** Bouton cœur connecté au FavoritesStore (ajout/retrait + état de chargement). */
export const FavoriteButton = observer(function FavoriteButton({
  kind,
  productId,
  variant = 'overlay',
}: FavoriteButtonProps) {
  const favorites = useFavoritesStore();
  const active = favorites.isFavorite(kind, productId);
  const pending = favorites.isPending(kind, productId);

  const handleClick = (e: React.MouseEvent) => {
    // Empêche la navigation quand le bouton est posé sur une carte cliquable.
    e.preventDefault();
    e.stopPropagation();
    void favorites.toggle(kind, productId);
  };

  const label = active ? 'Retirer des favoris' : 'Ajouter aux favoris';

  if (variant === 'inline') {
    return (
      <button
        type="button"
        onClick={handleClick}
        disabled={pending}
        aria-pressed={active}
        className={cn(
          'inline-flex items-center gap-2 rounded-xl border-[1.5px] px-4 py-[10px] font-body text-sm font-medium transition-all disabled:opacity-60',
          active
            ? 'border-fuchsia bg-blush text-fuchsia'
            : 'border-lin bg-white text-slate hover:border-fuchsia hover:text-fuchsia'
        )}
      >
        <HeartIcon filled={active} width={16} height={16} />
        {active ? 'En favori' : 'Ajouter aux favoris'}
      </button>
    );
  }

  return (
    <button
      type="button"
      onClick={handleClick}
      disabled={pending}
      aria-label={label}
      aria-pressed={active}
      title={label}
      className={cn(
        'absolute right-3 top-3 flex h-9 w-9 items-center justify-center rounded-full bg-white/90 shadow-card backdrop-blur transition-all hover:scale-105 disabled:opacity-60',
        active ? 'text-fuchsia' : 'text-slate hover:text-fuchsia'
      )}
    >
      <HeartIcon filled={active} width={18} height={18} />
    </button>
  );
});
