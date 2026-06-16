import { cn } from '../../lib/cn';
import { GemIcon } from '../../components/icons-admin';

interface ProductImageProps {
  src: string | null;
  alt: string;
  className?: string;
}

/** Affiche l'image d'un produit, ou un placeholder élégant si absente. */
export function ProductImage({ src, alt, className }: ProductImageProps) {
  if (src) {
    return <img src={src} alt={alt} className={cn('object-cover', className)} />;
  }
  return (
    <div
      className={cn(
        'flex items-center justify-center bg-blush text-fuchsia/50',
        className
      )}
      aria-label="Aucune image"
    >
      <GemIcon width={28} height={28} />
    </div>
  );
}
