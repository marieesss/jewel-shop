import type { AdminUser } from '../../api/admin';
import { formatDate, formatDateOnly } from '../../lib/format';

const ROLE_LABELS: Record<string, string> = {
  Admin: 'Administrateur',
  User: 'Utilisateur',
};

const COLUMNS = ['Prénom', 'Nom', 'Email', 'Rôle', 'Date de naissance', 'Inscrit le'];

interface UserTableProps {
  users: AdminUser[];
}

/** Tableau des inscrits (admin). */
export function UserTable({ users }: UserTableProps) {
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
          {users.map((user) => (
            <tr key={user.id} className="border-b border-mist/60 last:border-0">
              <td className="px-4 py-3">{user.name}</td>
              <td className="px-4 py-3">{user.surname}</td>
              <td className="px-4 py-3">{user.email}</td>
              <td className="px-4 py-3">{ROLE_LABELS[user.role] ?? user.role}</td>
              <td className="whitespace-nowrap px-4 py-3">{formatDateOnly(user.birthday)}</td>
              <td className="whitespace-nowrap px-4 py-3">{formatDate(user.createdAt)}</td>
            </tr>
          ))}
        </tbody>
      </table>
    </div>
  );
}
