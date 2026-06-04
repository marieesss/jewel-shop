# Jewel Front

Front-end React + TypeScript + MobX pour **JewelryShop**.
Implémente l'écran **« Login & Inscription »** (design Claude Design) connecté à l'API .NET.

## Stack

- **React 18** + **TypeScript** (Vite)
- **MobX** (`mobx` + `mobx-react-lite`) pour l'état d'authentification
- **Axios** pour les appels HTTP (injection automatique du Bearer token)

## Démarrage

```bash
npm install
npm run dev          # http://localhost:5173
```

L'API doit tourner en parallèle (profil `http` → `http://localhost:5030`) :

```bash
cd ../jewel-api
dotnet run --project src/JewelryShop.API
```

> Le CORS du back-end autorise `http://localhost:5173` par défaut.
> Pour changer l'origine : variable d'environnement `Cors__Origins`
> (valeurs séparées par des virgules).

## Configuration

`.env` :

```
VITE_API_URL=http://localhost:5030
```

## Structure

| Dossier | Rôle |
|---|---|
| `src/api/` | Client Axios + endpoints `auth` (`login`, `register`) |
| `src/stores/` | `AuthStore` (MobX) + contexte React (`useAuthStore`) |
| `src/components/` | `FormInput`, `DecoPanel`, icônes SVG |
| `src/pages/AuthPage.tsx` | Écran Connexion / Inscription (design) |
| `src/App.tsx` | Bascule connecté / non connecté |

## Flux d'authentification

1. L'utilisateur soumet le formulaire (connexion ou inscription).
2. `AuthStore` appelle `POST /api/auth/login` ou `/register`.
3. La réponse `{ token, userId, role }` est stockée (et le JWT persisté dans
   `localStorage` sous `jewel_token`).
4. Les requêtes suivantes envoient automatiquement `Authorization: Bearer <token>`.
