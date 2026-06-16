import { Link } from 'react-router-dom';
import { Badge } from '../../components/ui/Badge';
import { colorLabel, colorTone } from '../../api/products';
import { formatPrice } from '../../lib/format';
import { ProductImage } from './ProductImage';

interface ProductCardProps {
  to: string;
  name: string;
  color: string;
  price: number;
  imageUrl: string | null;
}

/** Vignette produit cliquable (liste). */
export function ProductCard({ to, name, color, price, imageUrl }: ProductCardProps) {
  return (
    <Link
      to={to}
      className="group flex flex-col overflow-hidden rounded-[18px] border border-lin bg-white shadow-card transition-all hover:-translate-y-1 hover:shadow-btn-hover"
    >
      <ProductImage
        src={imageUrl}
        alt={name}
        className="h-40 w-full transition-transform duration-300 group-hover:scale-[1.03]"
      />
      <div className="flex flex-1 flex-col gap-2 p-4">
        <div className="flex items-start justify-between gap-2">
          <p className="font-body text-sm font-semibold text-storm">{name}</p>
          <Badge tone={colorTone(color)}>{colorLabel(color)}</Badge>
        </div>
        <p className="mt-auto font-display text-lg font-medium text-fuchsia">
          {formatPrice(price)}
        </p>
      </div>
    </Link>
  );
}
