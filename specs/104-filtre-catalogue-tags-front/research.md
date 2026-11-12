# Recherche : filtrer le catalogue par tags dans l'interface

Aucun point du contexte technique n'était marqué « NEEDS CLARIFICATION ». Les deux questions de la spec ont été tranchées par la responsable (section Clarifications). Ce document consigne les choix de conception.

## R1. Source des tags et de leurs nombres

- **Décision** : nouvelle fonction `getTagCounts(kind)` dans `api/tags.ts`, qui appelle `GET /api/tags?kind=chain|charm` et renvoie `TagCountDto[]` (`{ id, name, slug, category, count }`).
- **Raison** : c'est le contrat livré par #103 (`docs/architecture.md` §2). Le nombre est global par type et ne dépend pas des autres filtres : un seul appel par liste, pas de rechargement quand la sélection change.
- **Alternatives écartées** : `getTags()` sans `kind` (pas de nombre) ; `GET /api/admin/tags` (réservé aux admins).
- Le type `TagCountKind = 'chain' | 'charm'` est défini dans `api/tags.ts`. Il a les mêmes valeurs que `ProductKind`, que la page passe directement. `api/` ne dépend pas de `stores/`.

## R2. Où vit la sélection

- **Décision** : dans l'adresse uniquement, paramètre `tags` (slugs séparés par des virgules), lu et écrit par `useSearchParams` dans `UserProductListPage`. Pas d'état React en double.
- **Raison** : une seule source de vérité. L'ouverture d'un lien partagé (story 2) et Précédent / Suivant (FR-005) marchent sans code de synchronisation.
- **Alternatives écartées** : un état local synchronisé dans les deux sens avec l'adresse, qui crée des boucles et des décalages ; un store MobX, inutile pour un état propre à une page.
- **Écriture** : `setSearchParams` part des paramètres courants et ne modifie que `tags`, pour ne pas écraser un autre paramètre futur. Chaque changement crée une entrée d'historique (`push`, le comportement par défaut), pour que Précédent annule le dernier filtre.

## R3. Lecture et écriture du paramètre

- **Décision** : fonctions pures dans `features/products/tagFiltering.ts` :
  - `parseTagSlugs(param: string | null): string[]` découpe sur `,`, retire les espaces autour, les valeurs vides et les doublons, et garde l'ordre ;
  - `withTagSlugs(params: URLSearchParams, slugs: string[]): URLSearchParams` renvoie une copie avec `tags` défini, ou supprimé si la liste est vide (FR-004) ;
  - `toggleTagSlug(slugs, slug)` ajoute ou retire un slug.
- **Raison** : testables avec Vitest sans rendu (CONTRIBUTING §5). Le nettoyage est un sous-ensemble de `TagFilter.ParseSlugs` côté API, qui reste l'arbitre (slug inconnu ignoré, casse et accents normalisés).
- **Slug inconnu** (FR-006) : il reste dans l'adresse, aucune case ne correspond, l'API l'ignore. Aucun message. Le front ne le retire pas de lui-même : cela réécrirait l'adresse sans action du client.
- **Même slug dans deux catégories** (« dore » en Couleur et en Matière) : l'API filtre sur les deux tags. Le front coche donc les deux cases ensemble, ce qui reflète fidèlement le filtre appliqué.

## R4. Tags affichés (décision 80 : masqués)

- **Décision** : `visibleTagGroups(tags, selectedSlugs)` regroupe par catégorie, dans l'ordre de `TAG_CATEGORIES` et dans l'ordre de l'API à l'intérieur, garde les tags dont `count > 0` **ou** dont le slug est coché, et retire les catégories vides.
- **Raison** : la spec masque les tags à 0, sauf s'ils sont cochés dans l'adresse (pour pouvoir les décocher). Une catégorie sans tag visible n'est pas affichée.
- **Alternative écartée** : réutiliser `groupTagsByCategory` de `tagSelection.ts`, qui trie par nom et filtre par recherche, ou celui de `lib/tagAdmin.ts`, typé `AdminTag` et qui garde les catégories vides. L'ordre de l'API est déjà le bon.

## R5. Retour en page 1 sans double chargement

- **Décision** : la page courante est mémorisée avec la valeur du paramètre `tags` à laquelle elle s'applique : `{ page, tagsParam }`. La page affichée vaut `page` si `tagsParam` est toujours celui de l'adresse, sinon 1. Fonction pure `currentPage(state, tagsParam)` testée.
- **Raison** : tout changement de sélection (case, « Effacer les filtres », Précédent / Suivant, lien) ramène en page 1, avec un seul appel de liste. Un `useEffect` qui remet la page à 1 provoquerait d'abord un appel sur l'ancienne page.
- **Alternative écartée** : mettre la page dans l'adresse, ce qui sort du périmètre (spec, hypothèses).

## R6. Intégration dans le panneau

- **Décision** : `ProductFilters` reçoit un `children` facultatif, rendu sous le bouton « Réinitialiser les filtres », séparé par un filet. La page y place `<TagFilters kind={kind} selectedSlugs={slugs} onChange={…} />`.
- **Raison** : `ProductFilters` reste purement présentationnel et ne connaît ni l'adresse ni l'API. « Réinitialiser » n'efface pas les tags (Q1) : placer le bloc des tags après, avec son propre bouton, rend la portée de chaque bouton évidente.
- **Alternative écartée** : passer `kind`, slugs et callback à `ProductFilters`, ce qui le couple à l'API des tags.

## R7. États du composant `TagFilters`

- Chargement : `Spinner` (« Chargement des tags… »).
- Erreur : `Alert` avec « Impossible de charger les tags. Vous pouvez filtrer la liste sans les tags. ». La liste et les autres filtres continuent (FR-011). Le message de l'API (`extractApiError`) n'est pas affiché : une erreur technique n'aide pas le client ici.
- Aucun tag visible : le composant ne rend rien.
- Bouton « Effacer les filtres » : affiché seulement quand au moins un tag est coché. Il appelle `onChange([])`.
- Libellé d'un tag : `« ${name} (${count}) »`, par `tagFilterLabel(tag)` (pure, testée).
- Les cases réutilisent `components/ui/Checkbox`, les titres de catégorie reprennent `TAG_CATEGORY_LABELS`.

## R8. Documentation

- `docs/architecture.md` §3 : nouvelle puce « Filtres par tags du catalogue » (paramètre `tags`, nombres globaux, masquage des tags à 0, bouton « Effacer les filtres », composant réutilisable).
- §5, ligne « Modèle catalogue » : « Le filtrage du catalogue reste à faire » devient « Filtrage du catalogue par tags fait côté front ; reste celui du parcours (#125) ».
