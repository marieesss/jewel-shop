import type { ChainLength } from '../../api/chainLengths';
import { Button } from '../../components/ui/Button';

const COLUMNS = ['Longueur (cm)', 'Libellé', 'Actions'];

interface ChainLengthTableProps {
  /** Longueurs dans l'ordre renvoyé par l'API (de la plus courte à la plus longue). */
  lengths: ChainLength[];
  /** Longueur dont une action est en cours : ses boutons sont désactivés. */
  actionInProgressId: number | null;
  onEdit: (length: ChainLength) => void;
  onDelete: (length: ChainLength) => void;
}

/** Tableau des longueurs de chaîne (admin), avec modification et suppression. */
export function ChainLengthTable({
  lengths,
  actionInProgressId,
  onEdit,
  onDelete,
}: ChainLengthTableProps) {
  return (
    <div className="overflow-x-auto rounded-[20px] border border-white/60 bg-white shadow-card">
      <table className="w-full font-body text-sm text-storm">
        <thead>
          <tr className="border-b border-mist text-left text-xs uppercase tracking-wide text-slate">
            {COLUMNS.map((column) => (
              <th key={column} scope="col" className="whitespace-nowrap px-4 py-3 font-semibold">
                {column}
              </th>
            ))}
          </tr>
        </thead>
        <tbody>
          {lengths.map((length) => {
            const isActionInProgress = actionInProgressId === length.id;
            return (
              <tr key={length.id} className="border-b border-mist/60 last:border-0">
                <td className="whitespace-nowrap px-4 py-3">{length.lengthCm}</td>
                <td className="px-4 py-3">{length.label}</td>
                <td className="whitespace-nowrap px-4 py-3">
                  <div className="flex gap-2">
                    <Button
                      variant="secondary"
                      className="px-3 py-1.5"
                      disabled={isActionInProgress}
                      onClick={() => onEdit(length)}
                    >
                      Modifier
                    </Button>
                    <Button
                      variant="ghost"
                      className="px-3 py-1.5"
                      disabled={isActionInProgress}
                      onClick={() => onDelete(length)}
                    >
                      Supprimer
                    </Button>
                  </div>
                </td>
              </tr>
            );
          })}
        </tbody>
      </table>
    </div>
  );
}
