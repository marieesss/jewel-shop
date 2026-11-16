# Contrat : paramètre d'adresse et composant `TagFilters`

## Paramètre d'adresse `tags`

| Page | Exemple |
|---|---|
| `/charms` | `/charms?tags=mer,dore` |
| `/chains` | `/chains?tags=plage` |

- Valeur : slugs séparés par une virgule, sans espace, dans l'ordre où ils ont été cochés.
- Absent quand aucun tag n'est coché (jamais `tags=` vide écrit par le front).
- À la lecture : valeurs vides, espaces autour et doublons ignorés. Un slug inconnu est conservé et ne coche rien.
- Chaque changement ajoute une entrée d'historique (Précédent revient à la sélection précédente).
- Les autres paramètres éventuels de l'adresse sont conservés.
- Changer la sélection, par quelque moyen que ce soit, ramène la liste en page 1.

Ce format est visible par le client (liens partagés) : le modifier est un changement cassant pour les liens existants.

## Composant `features/products/TagFilters`

Réutilisable par le parcours de création (#125). Il ne lit ni n'écrit l'adresse lui-même.

```ts
interface TagFiltersProps {
  /** Type d'article dont on affiche les tags et les nombres. */
  kind: TagCountKind;            // 'chain' | 'charm'
  /** Slugs cochés. */
  selectedSlugs: string[];
  /** Nouvelle liste de slugs cochés (vide pour « Effacer les filtres »). */
  onChange: (slugs: string[]) => void;
}
```

Comportement :

| État | Rendu |
|---|---|
| Chargement des tags | `Spinner` « Chargement des tags… » |
| Échec du chargement | `Alert` « Impossible de charger les tags. Vous pouvez filtrer la liste sans les tags. » |
| Aucun tag visible | rien |
| Tags visibles | un titre par catégorie (Thème, Couleur, Matière, Occasion), une case « {nom} ({nombre}) » par tag |
| Au moins un tag coché | bouton « Effacer les filtres », qui appelle `onChange([])` |

Visibilité d'un tag : `count > 0`, ou slug présent dans `selectedSlugs` (décision 80, tags à 0 masqués).

## `ProductFilters` (modifié)

Nouveau `children?: ReactNode`, rendu en bas du panneau, sous « Réinitialiser les filtres ». Le bouton « Réinitialiser les filtres » ne change pas et n'efface pas les tags.
