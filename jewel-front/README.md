# mycharmly : front

Interface client et admin de mycharmly : catalogue des chaînes et breloques, favoris, et espace d'administration des produits.

## Pile

React 18, TypeScript, Vite, MobX, Tailwind, axios, react-router 6.

## Prérequis

Node.js 20.

## Démarrage

```bash
npm ci
npm run dev          # http://localhost:5173
```

L'API doit tourner en parallèle sur http://localhost:5030 : voir [`CONTRIBUTING.md`](../CONTRIBUTING.md) §2.

## Scripts

| Script                 | Rôle                                                 |
| ---------------------- | ---------------------------------------------------- |
| `npm run dev`          | Serveur de développement Vite                        |
| `npm run build`        | Vérification des types (`tsc -b`) puis build de prod |
| `npm run preview`      | Sert le build de production en local                 |
| `npm run lint`         | ESLint                                               |
| `npm run format`       | Formate le code avec Prettier                        |
| `npm run format:check` | Vérifie le formatage Prettier                        |
| `npm test`             | Tests Vitest                                         |

## Configuration

| Variable       | Rôle                                                                                                                                             |
| -------------- | ------------------------------------------------------------------------------------------------------------------------------------------------ |
| `VITE_API_URL` | URL de l'API, lue **au build**. Vide : URL relatives (`/api/...`), cas de la production. Non définie (dev) : `http://localhost:5030` par défaut. |

## Structure de `src/`

| Dossier          | Rôle                                                                               |
| ---------------- | ---------------------------------------------------------------------------------- |
| `api/`           | Client axios, intercepteurs d'authentification, appels HTTP et types des DTO       |
| `stores/`        | État MobX (`AuthStore`, `ProductAdminStore`, `FavoritesStore`) et hooks d'accès    |
| `routes/`        | Guards des routes protégées (`ProtectedRoute`, `AdminRoute`)                       |
| `pages/`         | Pages routées : `AuthPage`, espace client (`user/`) et admin (`admin/`)            |
| `features/`      | Composants métier par domaine (`admin`, `auth`, `products`, `favorites`, `user`)   |
| `components/ui/` | Composants génériques, sans appel API (`Button`, `TextField`, `Pagination`…)       |
| `hooks/`         | Hooks partagés (`useAsync`)                                                        |
| `lib/`           | Utilitaires (`cn`, formatage, redirection après connexion, règles du mot de passe) |

Les routes sont déclarées dans `src/App.tsx`.

## Pour aller plus loin

- [`CONTRIBUTING.md`](../CONTRIBUTING.md) : conventions, vérifications avant de pousser, déroulé d'une contribution.
- [`docs/architecture.md`](../docs/architecture.md) : architecture du front et de l'API.
