import { observer } from 'mobx-react-lite';
import { Link } from 'react-router-dom';
import { useAuthStore } from '../stores/context';
import { Button } from '../components/ui/Button';

/** Écran d'accueil après authentification (utilisateur connecté). */
export const HomePage = observer(function HomePage() {
  const auth = useAuthStore();

  return (
    <div className="flex h-screen flex-col items-center justify-center gap-4 bg-creme font-body text-storm">
      <h1 className="font-comfortaa text-[40px] font-light">Bienvenue ✨</h1>
      <p className="text-slate">
        Connecté · utilisateur #{auth.userId} · rôle {auth.role}
      </p>

      <div className="mt-2 flex gap-3">
        {auth.isAdmin && (
          <Link to="/admin">
            <Button>Espace administration</Button>
          </Link>
        )}
        <Button variant="secondary" onClick={() => auth.logout()}>
          Se déconnecter
        </Button>
      </div>
    </div>
  );
});
