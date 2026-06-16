import { Button } from './Button';

interface PaginationProps {
  page: number;
  totalPages: number;
  hasPrevious: boolean;
  hasNext: boolean;
  onChange: (page: number) => void;
}

/** Contrôle de pagination « précédent / suivant » réutilisable. */
export function Pagination({ page, totalPages, hasPrevious, hasNext, onChange }: PaginationProps) {
  if (totalPages <= 1) return null;

  return (
    <div className="mt-6 flex items-center justify-center gap-4">
      <Button
        variant="secondary"
        disabled={!hasPrevious}
        onClick={() => onChange(page - 1)}
        className="px-4 py-2"
      >
        Précédent
      </Button>
      <span className="font-body text-[13px] text-slate">
        Page {page} / {totalPages}
      </span>
      <Button
        variant="secondary"
        disabled={!hasNext}
        onClick={() => onChange(page + 1)}
        className="px-4 py-2"
      >
        Suivant
      </Button>
    </div>
  );
}
