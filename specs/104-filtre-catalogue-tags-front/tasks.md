---

description: "Liste des tâches : filtrer le catalogue par tags dans l'interface"
---

# Tasks: Filtrer le catalogue par tags dans l'interface

**Input**: documents de `specs/104-filtre-catalogue-tags-front/` (plan.md, spec.md, research.md, data-model.md, contracts/, quickstart.md)

**Prerequisites**: plan.md, spec.md, research.md, data-model.md, contracts/

**Tests**: demandés par la constitution (principe V) et la Definition of Done de l'issue. Les fonctions pures sont testées avec Vitest, dans le même commit que le code qu'elles couvrent, en écrivant les tests d'abord. Les composants sont vérifiés à la main (quickstart), comme le reste du front.

**Organization**: une tâche = un commit Conventional Commits en français, fait à la main, qui passe `npm run lint && npm run format:check && npm run build && npm test` dans `jewel-front/`. Les tâches de vérification (sans fichier modifié) ne donnent pas de commit.

## Format: `[ID] [P?] [Story] Description`

- **[P]** : peut être faite en parallèle (fichiers différents, aucune dépendance en cours)
- **[Story]** : story de la spec (US1 à US4)
- Chemins relatifs à la racine du dépôt

---

## Phase 1: Setup (Shared Infrastructure)

Aucune tâche : le projet front, Vitest, ESLint et Prettier existent. Aucune dépendance à ajouter (plan, Technical Context).

---

## Phase 2: Foundational (Blocking Prerequisites)

**Purpose**: appel d'API et lecture / écriture du paramètre d'adresse, utilisés par toutes les stories.

**⚠️ CRITICAL**: aucune story ne commence avant la fin de cette phase.

- [X] T001 [P] Ajouter l'appel des tags avec leur nombre d'articles dans `jewel-front/src/api/tags.ts` et le paramètre `tags` dans `jewel-front/src/api/products.ts`. Dans `tags.ts` : `export type TagCountKind = 'chain' | 'charm';` (valeur du paramètre `kind`, mêmes valeurs que `ProductKind`, sans importer `stores/`) ; `export interface TagCountDto extends TagDto { count: number; }` (« nombre global d'articles du type demandé qui portent le tag (≥ 0) ») ; `/** GET /api/tags?kind=chain|charm → tags triés par catégorie puis par nom, avec leur nombre global d'articles */ export async function getTagCounts(kind: TagCountKind): Promise<TagCountDto[]>` qui appelle `api.get<TagCountDto[]>('/api/tags', { params: { kind } })`. Dans `products.ts`, ajouter à `ProductListParams` : `/** Slugs de tags séparés par des virgules (OU dans une catégorie, ET entre catégories). */ tags?: string;`. Commit : `feat(catalog): ajouter l'appel des tags avec leur nombre d'articles`.
- [X] T002 [P] Créer les fonctions pures du paramètre d'adresse dans `jewel-front/src/features/products/tagFiltering.ts`, avec leurs tests dans `jewel-front/src/features/products/tagFiltering.test.ts` (noms de tests en phrases françaises, écrits d'abord). Constantes nommées : `export const TAGS_PARAM = 'tags';` et `const TAG_SLUG_SEPARATOR = ',';`. Fonctions :
  - `parseTagSlugs(param: string | null): string[]` : découpe sur `,`, retire les espaces autour, les valeurs vides et les doublons, garde l'ordre. Tests : `null` → `[]` ; `''` → `[]` ; `'mer,dore'` → `['mer','dore']` ; `' mer , ,dore,mer,'` → `['mer','dore']`.
  - `serializeTagSlugs(slugs: string[]): string | undefined` : `slugs.join(',')`, ou `undefined` si la liste est vide (valeur passée à `ProductListParams.tags`). Tests : `[]` → `undefined` ; `['mer','dore']` → `'mer,dore'`.
  - `withTagSlugs(params: URLSearchParams, slugs: string[]): URLSearchParams` : renvoie une **copie** avec `tags` défini, ou supprimé si `slugs` est vide (« jamais `tags=` vide écrit par le front »). Tests : ajout ; remplacement ; suppression quand vide ; un autre paramètre (`foo=1`) est conservé ; l'objet reçu n'est pas modifié.
  - `toggleTagSlug(slugs: string[], slug: string): string[]` : ajoute à la fin si absent, retire sinon. Tests : ajout en fin ; retrait ; liste d'origine inchangée.
  - `interface TagFilteredPage { page: number; tagsParam: string | null; }` et `currentPage(state: TagFilteredPage, tagsParam: string | null): number` : `state.page` si `state.tagsParam === tagsParam`, sinon `1` (research R5). Tests : même paramètre → page mémorisée ; paramètre différent → 1 ; `null` contre `'mer'` → 1.
  Commit : `feat(catalog): lire et écrire les tags du filtre dans l'adresse`.

**Checkpoint**: `npm test` passe ; rien n'est encore visible.

---

## Phase 3: User Story 1 - Filtrer une liste en cochant des tags (Priority: P1) 🎯 MVP

**Goal**: sur `/charms` et `/chains`, les tags du type affiché sont listés par catégorie avec leur nombre ; cocher / décocher met à jour `?tags=` et recharge la liste en page 1.

**Independent Test**: ouvrir `/charms`, cocher « Mer » : l'adresse devient `/charms?tags=mer`, la liste ne montre que les breloques portant « Mer », en page 1 (quickstart, parcours 1 à 4, 11 et 12).

- [X] T003 [US1] Ajouter le regroupement et le libellé des tags dans `jewel-front/src/features/products/tagFiltering.ts`, tests d'abord dans `jewel-front/src/features/products/tagFiltering.test.ts` :
  - `export interface VisibleTagGroup { category: TagCategory; label: string; tags: TagCountDto[]; }` ;
  - `visibleTagGroups(tags: TagCountDto[], selectedSlugs: string[]): VisibleTagGroup[]` : une entrée par catégorie de `TAG_CATEGORIES`, dans cet ordre, `label` tiré de `TAG_CATEGORY_LABELS`, tags dans l'ordre reçu de l'API ; « une catégorie sans tag visible est absente » ; « un tag d'une catégorie inconnue n'est pas affiché ». À ce stade tous les tags connus sont visibles (le masquage des tags à 0 est T008) : le paramètre `selectedSlugs` est déjà présent pour T008. Tests : ordre des catégories même si l'API les mélange ; ordre des tags conservé ; catégorie vide absente ; catégorie inconnue ignorée.
  - `tagFilterLabel(tag: TagCountDto): string` → `` `${tag.name} (${tag.count})` ``. Test : « Mer (10) ».
  Commit : `feat(catalog): regrouper les tags du filtre par catégorie`.
- [X] T004 [US1] Créer le bloc réutilisable `jewel-front/src/features/products/TagFilters.tsx` (export nommé `TagFilters`, calqué sur `TagPicker.tsx`), conforme à `specs/104-filtre-catalogue-tags-front/contracts/tag-filters-ui.md` : props `{ kind: TagCountKind; selectedSlugs: string[]; onChange: (slugs: string[]) => void }`, avec le JSDoc du contrat. Il ne lit ni n'écrit l'adresse. Il charge `useAsync(() => getTagCounts(kind), [kind])` ; pendant le chargement `Spinner` avec le libellé « Chargement des tags… » ; en erreur `Alert` avec le texte constant `TAGS_LOAD_ERROR_MESSAGE = 'Impossible de charger les tags. Vous pouvez filtrer la liste sans les tags.'` (déclaré dans `tagFiltering.ts`, le message de l'API n'est pas affiché) ; sinon `visibleTagGroups(data, selectedSlugs)` ; aucun groupe → `null`. Chaque groupe : un titre (`group.label`) puis un `Checkbox` (`components/ui/Checkbox`) par tag, `label={tagFilterLabel(tag)}`, `checked={selectedSlugs.includes(tag.slug)}` (deux tags de même slug se cochent ensemble, research R3), `onChange={() => onChange(toggleTagSlug(selectedSlugs, tag.slug))}`. Style Tailwind repris de `ProductFilters` / `TagPicker` (`cn()` si besoin). Commit : `feat(catalog): ajouter le bloc de filtres par tags`.
- [X] T005 [US1] Intégrer le bloc au catalogue : dans `jewel-front/src/features/products/ProductFilters.tsx`, ajouter `children?: ReactNode` aux props, rendu en bas du panneau, sous le bouton « Réinitialiser les filtres », séparé par un filet (`border-t border-lin pt-4`), seulement si `children` est fourni ; le bouton « Réinitialiser les filtres » et `resetFilters` ne changent pas et n'effacent pas les tags. Dans `jewel-front/src/pages/user/UserProductListPage.tsx` :
  - `const [searchParams, setSearchParams] = useSearchParams();` (`react-router-dom`), `const tagsParam = searchParams.get(TAGS_PARAM);`, `const tagSlugs = parseTagSlugs(tagsParam);` ;
  - remplacer `useState(1)` par `useState<TagFilteredPage>({ page: 1, tagsParam })` et calculer `const page = currentPage(pageState, tagsParam);` ; `setPage(n)` devient `setPageState({ page: n, tagsParam })` ; l'effet sur `kind` et `patchFilters` / `resetFilters` remettent `{ page: 1, tagsParam }` ;
  - `params.tags = serializeTagSlugs(tagSlugs)` et ajouter `tagsParam` aux dépendances de `useAsync` ;
  - `const changeTags = (slugs: string[]) => setSearchParams((prev) => withTagSlugs(prev, slugs));` (navigation `push` par défaut, pour Précédent / Suivant) ;
  - `<ProductFilters …><TagFilters kind={kind} selectedSlugs={tagSlugs} onChange={changeTags} /></ProductFilters>`.
  Vérifier dans l'onglet réseau qu'un changement de tag en page 2 ne déclenche qu'un appel de liste, en page 1. Commit : `feat(catalog): filtrer les listes de chaînes et de breloques par tags`.
- [ ] T006 [US1] Vérifier à la main les parcours 1 à 4, 6, 11 et 12 de `specs/104-filtre-catalogue-tags-front/quickstart.md` (aucun commit).

**Checkpoint**: la story 1 est utilisable seule ; la liste se filtre et l'adresse porte `?tags=`.

---

## Phase 4: User Story 2 - Retrouver ses filtres depuis un lien partagé (Priority: P2)

**Goal**: une adresse `?tags=` ouverte directement, ou atteinte par Précédent / Suivant, recoche les filtres et filtre la liste.

**Independent Test**: ouvrir `/charms?tags=mer,dore` dans un nouvel onglet : « Mer » et « Doré » sont cochés, la liste est filtrée.

Aucun code supplémentaire : l'adresse est la seule source de la sélection (research R2), lue par `parseTagSlugs` (T002) et la page (T005). Un slug inconnu ne coche rien et l'API l'ignore (R3).

- [ ] T007 [US2] Vérifier à la main les parcours 5, 8 et 9 de `specs/104-filtre-catalogue-tags-front/quickstart.md` (Précédent du navigateur, lien ouvert dans un nouvel onglet, slug inconnu sans erreur) ; corriger dans `jewel-front/src/pages/user/UserProductListPage.tsx` si un écart apparaît (commit `fix(catalog): …` le cas échéant).

**Checkpoint**: stories 1 et 2 validées.

---

## Phase 5: User Story 3 - Effacer les filtres (Priority: P3)

**Goal**: un clic sur « Effacer les filtres » décoche tous les tags et retire `tags` de l'adresse, sans toucher à la couleur, au prix ni au stock.

**Independent Test**: cocher deux tags et choisir une couleur, cliquer « Effacer les filtres » : plus aucun tag coché, `tags` absent de l'adresse, la couleur reste appliquée, page 1.

- [X] T008 [US3] Dans `jewel-front/src/features/products/TagFilters.tsx`, ajouter sous les groupes un `Button` (`components/ui/Button`, `variant="ghost"`, même style que « Réinitialiser les filtres » de `ProductFilters`) libellé « Effacer les filtres », affiché seulement si `selectedSlugs.length > 0`, qui appelle `onChange([])`. Le bouton reste affiché si les tags cochés ne sont pas visibles (slug inconnu seul), pour pouvoir nettoyer l'adresse : dans ce cas le bloc ne rend que le bouton au lieu de `null`. Vérifier les parcours 6 et 7 de `quickstart.md`. Commit : `feat(catalog): effacer les filtres par tags`.

**Checkpoint**: stories 1 à 3 validées.

---

## Phase 6: User Story 4 - Ne pas proposer de filtre menant à une liste vide (Priority: P3)

**Goal**: décision 80, les tags dont le nombre vaut 0 sont masqués, sauf s'ils sont cochés dans l'adresse.

**Independent Test**: un tag porté par aucune breloque n'apparaît pas sur `/charms` ; ouvert par `/charms?tags=<son-slug>`, il apparaît coché et disparaît une fois décoché.

- [X] T009 [US4] Dans `jewel-front/src/features/products/tagFiltering.ts`, faire appliquer par `visibleTagGroups` la règle « un tag est visible si `count > 0` ou si son slug est coché » (variable au nom explicite, par exemple `isOffered`), tests d'abord dans `jewel-front/src/features/products/tagFiltering.test.ts` : tag à 0 non coché masqué ; tag à 0 coché visible ; catégorie dont tous les tags sont à 0 absente ; même catégorie présente si l'un de ses tags à 0 est coché. Vérifier le parcours 10 de `quickstart.md`. Commit : `feat(catalog): masquer les tags sans article dans les filtres`.

**Checkpoint**: toutes les stories sont validées.

---

## Phase 7: Polish & Cross-Cutting Concerns

- [X] T010 [P] Mettre à jour `docs/architecture.md` : au §3 (front), ajouter une puce « Filtres par tags du catalogue » (`/charms` et `/chains` : paramètre d'adresse `tags`, slugs séparés par des virgules, absent sans tag coché ; tags de `GET /api/tags?kind=` par catégorie, libellé « {nom} ({nombre}) », nombre global ; tags à 0 masqués sauf s'ils sont cochés ; « Effacer les filtres » ne retire que les tags, « Réinitialiser les filtres » ne les touche pas ; retour en page 1 à chaque changement ; bloc `features/products/TagFilters` réutilisable par #125 ; message en cas d'échec du chargement). Au §5, ligne « Modèle catalogue », remplacer « Le filtrage du catalogue reste à faire. » par « Filtrage du catalogue par tags fait côté front (`/charms`, `/chains`) ; celui des étapes du parcours reste à faire (#125). ». Commit : `docs: documenter le filtre du catalogue par tags`.
- [ ] T011 Lancer `cd jewel-front && npm run lint && npm run format:check && npm run build && npm test`, puis dérouler les 13 parcours de `specs/104-filtre-catalogue-tags-front/quickstart.md` (dont le 13 : échec du chargement des tags) ; un reformatage éventuel va dans un commit séparé `chore: …` sans changement de comportement.
- [ ] T012 Pousser la branche `feat/104-filtre-catalogue-tags-front` et ouvrir la PR vers `main` (titre `feat(catalog): filtrer le catalogue par tags dans l'interface`), avec résumé et choix faits (tags à 0 masqués, effacement limité aux tags, slugs identiques cochés ensemble, message de chargement), `Closes #104`, section « Impact production » (migration : aucune ; API : aucun changement ; visibilité : visible dès le merge ; configuration : aucune ; retour arrière : revert de la PR), « Comment tester » (tests Vitest de `tagFiltering.test.ts`, parcours du quickstart) et la checklist DoD de l'issue cochée. Le dossier `specs/104-filtre-catalogue-tags-front/` fait partie de la PR.

---

## Dependencies & Execution Order

### Phase Dependencies

- **Phase 2** : aucune dépendance ; bloque toutes les stories.
- **US1 (phase 3)** : après T001 et T002.
- **US2 (phase 4)** : vérification seulement, après T005.
- **US3 (phase 5)** : après T004 (même composant). Indépendante de US2 et US4.
- **US4 (phase 6)** : après T003 (même fonction). Indépendante de US2 et US3.
- **Polish** : T010 peut se faire à tout moment après la phase 2 ; T011 et T012 après toutes les stories.

### Within Each Story

- Tests écrits avant le code de la fonction pure, dans le même commit.
- Fonctions pures → composant → intégration à la page.

### Parallel Opportunities

- T001 et T002 : fichiers différents (`api/` et `features/products/tagFiltering.ts`).
- T008 (US3, `TagFilters.tsx`) et T009 (US4, `tagFiltering.ts`) une fois US1 terminée.
- T010 (documentation) en parallèle de toute tâche de code.

---

## Parallel Example: Phase 2

```text
T001 — jewel-front/src/api/tags.ts + jewel-front/src/api/products.ts
T002 — jewel-front/src/features/products/tagFiltering.ts + tagFiltering.test.ts
```

## Parallel Example: après US1

```text
T008 [US3] — jewel-front/src/features/products/TagFilters.tsx
T009 [US4] — jewel-front/src/features/products/tagFiltering.ts + tagFiltering.test.ts
T010       — docs/architecture.md
```

---

## Implementation Strategy

### MVP First (User Story 1 Only)

1. Phase 2 : T001, T002.
2. Phase 3 : T003 à T006.
3. **Arrêt et validation** : la liste se filtre par tags et l'adresse porte la sélection.

### Incremental Delivery

1. US1 → filtre fonctionnel (MVP).
2. US2 → vérification des liens partagés et de l'historique.
3. US3 → bouton « Effacer les filtres ».
4. US4 → masquage des tags à 0.
5. Documentation, vérifications, PR unique pour l'issue #104 (la fonctionnalité n'est livrée qu'une fois la PR mergée : aucun masquage intermédiaire nécessaire).
