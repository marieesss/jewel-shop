import { useEffect } from 'react';
import { observer } from 'mobx-react-lite';
import { useLocation } from 'react-router-dom';
import { Alert } from '../../components/ui/Alert';
import { useFavoritesStore } from '../../stores/context';

interface FavoriteErrorAlertProps {
  className?: string;
}

/**
 * Erreur du FavoritesStore (ajout ou retrait échoué), affichée sur la page où se trouve le bouton.
 * Elle disparaît au clic suivant sur un bouton favori, ou dès que l'utilisateur change de page.
 */
export const FavoriteErrorAlert = observer(function FavoriteErrorAlert({
  className,
}: FavoriteErrorAlertProps) {
  const favorites = useFavoritesStore();
  const { pathname } = useLocation();

  // Le store est partagé entre les pages : sans ce nettoyage, l'erreur suivrait l'utilisateur.
  // Le chemin est une dépendance car React Router réutilise la page entre /charms et /chains.
  useEffect(() => () => favorites.clearError(), [favorites, pathname]);

  if (!favorites.error) return null;
  return <Alert className={className}>{favorites.error}</Alert>;
});
