import { observer } from 'mobx-react-lite';
import { StoreProvider, rootStore, useAuthStore } from './stores/context';
import { AuthPage } from './pages/AuthPage';

/**
 * Vue affichée une fois connecté. Volontairement minimale : l'objet de ce
 * livrable est l'écran « Login & Inscription ». Elle confirme que le flux
 * d'authentification fonctionne de bout en bout avec le back-end.
 */
const Authenticated = observer(function Authenticated() {
  const auth = useAuthStore();
  return (
    <div className="flex h-screen flex-col items-center justify-center gap-4 bg-creme font-body text-storm">
      <h1 className="font-comfortaa text-[40px] font-light">Bienvenue ✨</h1>
      <p className="text-slate">
        Connecté · utilisateur #{auth.userId} · rôle {auth.role}
      </p>
      <button
        onClick={() => auth.logout()}
        className="rounded-xl bg-storm px-7 py-3 font-body text-sm font-medium text-white shadow-btn transition-all hover:-translate-y-0.5 hover:shadow-btn-hover"
      >
        Se déconnecter
      </button>
    </div>
  );
});

const Root = observer(function Root() {
  const auth = useAuthStore();
  return auth.isAuthenticated ? <Authenticated /> : <AuthPage />;
});

export default function App() {
  return (
    <StoreProvider value={rootStore}>
      <Root />
    </StoreProvider>
  );
}
