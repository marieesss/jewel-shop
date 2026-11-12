# Implementation Plan: Filtrer le catalogue par tags dans l'interface

**Branch**: `feat/104-filtre-catalogue-tags-front` | **Date**: 2026-11-12 | **Spec**: [spec.md](spec.md)

**Input**: Feature specification from `specs/104-filtre-catalogue-tags-front/spec.md`

## Summary

Ajouter aux listes `/charms` et `/chains` un bloc de filtres par tags, groupés par catégorie avec leur nombre global d'articles (« Mer (10) »), dont la sélection vit dans le paramètre d'adresse `?tags=`. Le front consomme l'API livrée par #103 (`GET /api/tags?kind=…` et le paramètre `tags` des listes), sans aucun changement côté back. Le bloc (`features/products/TagFilters`) est autonome (type de produit, slugs cochés, callback) pour être repris tel quel par le parcours de création (#125). La logique (lecture et écriture du paramètre, bascule d'un slug, tags visibles selon la décision 80) est en fonctions pures testées avec Vitest.

## Technical Context

**Language/Version**: TypeScript 5, React 18, Node.js 20

**Primary Dependencies**: Vite, react-router-dom 6 (`useSearchParams`), axios (client `api`), Tailwind. MobX n'est pas utilisé : l'état vit dans l'adresse. Aucune nouvelle dépendance.

**Storage**: N/A (état dans l'adresse de la page)

**Testing**: Vitest (`npm test`) sur les fonctions pures, à côté du fichier testé ; vérification manuelle des parcours (quickstart)

**Target Platform**: navigateurs récents, bureau et mobile (panneau de filtres existant, empilé en mobile)

**Project Type**: application web (API .NET + front React) ; seul le front change

**Performance Goals**: un seul appel `GET /api/tags?kind=` par affichage de liste, un seul appel de liste par changement de sélection (pas de double chargement au retour en page 1)

**Constraints**: API inchangée (#103) ; messages en français au vouvoiement ; visible dès le merge

**Scale/Scope**: quelques dizaines de tags répartis en 4 catégories ; 2 pages concernées ; 1 composant, 1 module de fonctions pures, 1 fonction d'API, docs

## Constitution Check

*GATE: Must pass before Phase 0 research. Re-check after Phase 1 design.*

| Principe | Vérification | Statut |
|---|---|---|
| I. L'issue est la source du besoin | Issue #104 `ready`, dépendance #103 fermée. Dossier `specs/104-filtre-catalogue-tags-front`. Les deux points ouverts (portée d'« Effacer les filtres », décision 80) ont été tranchés par la responsable le 2026-11-12 et notés dans la spec. | ✅ |
| II. On se calque sur l'existant | Fonctionnalité la plus proche : `features/products/TagPicker` (tags par catégorie en cases à cocher, chargés par `useAsync`) et sa logique pure `tagSelection.ts`. Le nouveau bloc reprend ce découpage (`TagFilters.tsx` + `tagFilters.ts` + `tagFilters.test.ts`). Appel d'API dans `api/tags.ts`, documenté par sa route. Lecture de l'adresse par `useSearchParams`, déjà utilisé (`AuthPage`, `RegisterPage`). Aucune nouvelle dépendance ni nouveau motif. | ✅ |
| III. Une validation, deux côtés | Aucune commande ni formulaire envoyé. Le nettoyage des slugs côté front (vides, doublons) est un sous-ensemble de `TagFilter.ParseSlugs` côté API, qui reste l'arbitre. | ✅ (N/A) |
| IV. Rien ne casse la production | Aucune migration, aucun changement d'API. Fonctionnalité terminée par cette issue : visible dès le merge, comme l'indique l'issue. Aucun secret. | ✅ |
| V. Chaque étape est testée | Fonctions pures testées (lecture/écriture du paramètre, bascule, tags visibles, page courante). Chaque tâche est un commit qui passe lint, format, build et tests. | ✅ |
| VI. Le code s'explique | Nom du paramètre (`tags`) et séparateur (`,`) en constantes nommées. Pas de commentaire paraphrasant. | ✅ |
| VII. Le français, partout où on lit | Libellés « Effacer les filtres », « Thème »…, message d'erreur de chargement au vouvoiement. Spec, plan et tâches en français. | ✅ |
| VIII. Le périmètre de l'issue | Seul le filtre par tags passe dans l'adresse ; les autres filtres et la page restent comme aujourd'hui. Le parcours (#125) n'est pas touché. Ni `deploy/` ni workflows. | ✅ |

**Re-check après la phase 1** : le design (voir [research.md](research.md), [data-model.md](data-model.md), [contracts/](contracts/)) n'introduit ni dépendance, ni store, ni changement d'API. Aucun écart : la section Complexity Tracking reste vide.

## Project Structure

### Documentation (this feature)

```text
specs/104-filtre-catalogue-tags-front/
├── spec.md
├── plan.md              # ce fichier
├── research.md          # phase 0
├── data-model.md        # phase 1
├── quickstart.md        # phase 1
├── contracts/
│   ├── api-consumed.md  # endpoints de #103 utilisés (inchangés)
│   └── tag-filters-ui.md # paramètre d'adresse et interface du composant TagFilters
├── checklists/
│   └── requirements.md
└── tasks.md             # phase 2 (/speckit-tasks)
```

### Source Code (repository root)

```text
jewel-front/src/
├── api/
│   ├── tags.ts                     # + TagCountKind, TagCountDto, getTagCounts(kind)
│   └── products.ts                 # ProductListParams : + tags?: string
├── features/products/
│   ├── tagFilters.ts               # nouveau : fonctions pures (paramètre ?tags=, bascule, tags visibles)
│   ├── tagFilters.test.ts          # nouveau : tests Vitest
│   ├── TagFilters.tsx              # nouveau : bloc réutilisable (#125)
│   └── ProductFilters.tsx          # + emplacement `children` sous les filtres existants
└── pages/user/
    └── UserProductListPage.tsx     # lit/écrit ?tags=, passe tags à la liste, page 1 à chaque changement

docs/architecture.md                # §3 front (catalogue client) et §5 roadmap (filtrage catalogue fait)
```

**Structure Decision**: application web existante ; seul `jewel-front/` et `docs/architecture.md` changent. Le composant va dans `features/products/` à côté de `TagPicker` (il appelle l'API, il ne peut donc pas aller dans `components/ui/`). Les fonctions pures restent à côté, comme `tagSelection.ts`.

## Complexity Tracking

Aucun écart à justifier.
