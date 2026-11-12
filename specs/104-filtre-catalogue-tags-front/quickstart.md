# Quickstart : valider le filtre du catalogue par tags

## Prérequis

- API, base et front lancés en local (`CONTRIBUTING.md` §2, « Hors Docker ») : front sur http://localhost:5173.
- Un compte admin (§2, « Premier compte admin en local ») et un compte client (le catalogue exige une session).
- Données, créées depuis l'admin (`/admin/tags` et formulaires produit) :
  - tags « Mer » et « Plage » (Thème), « Doré » (Couleur), « Mariage » (Occasion) ;
  - breloque A : Mer + Doré ; breloque B : Plage ; breloque C : Mer ; chaîne D : Plage ;
  - « Mariage » n'est porté par aucune breloque ni aucune chaîne.

## Vérifications automatiques

```bash
cd jewel-front && npm run lint && npm run format:check && npm run build && npm test
```

`tagFilters.test.ts` doit couvrir : lecture du paramètre (vide, absent, espaces, doublons), écriture (ajout, retrait, suppression du paramètre, autres paramètres conservés), bascule, tags visibles (tag à 0 masqué, tag à 0 coché visible, catégorie vide absente, ordre), libellé « Mer (2) » et retour en page 1.

## Parcours manuels

| # | Action | Résultat attendu |
|---|---|---|
| 1 | Ouvrir `/charms` | Thème : « Mer (2) », « Plage (1) » ; Couleur : « Doré (1) ». Ni « Mariage » ni la catégorie Occasion. |
| 2 | Cocher « Mer » | Adresse `/charms?tags=mer`, liste A et C, page 1. |
| 3 | Cocher « Plage » | `tags=mer,plage`, liste A, B, C. |
| 4 | Cocher « Doré » | liste A seule (Mer ou Plage, et Doré). |
| 5 | Précédent du navigateur | « Doré » décoché, liste A, B, C. |
| 6 | Choisir la couleur « Or » puis « Réinitialiser les filtres » | la couleur est remise à zéro, les tags restent cochés. |
| 7 | « Effacer les filtres » | `tags` disparaît de l'adresse, plus rien n'est coché, liste complète, le bouton disparaît. |
| 8 | Ouvrir `/charms?tags=mer,dore` dans un nouvel onglet | « Mer » et « Doré » cochés, liste A. |
| 9 | Ouvrir `/charms?tags=mer,inconnu` | « Mer » coché, liste A et C, aucune erreur. |
| 10 | Ouvrir `/charms?tags=mariage` | « Mariage (0) » affiché coché sous Occasion ; le décocher le fait disparaître. |
| 11 | Avec 13 breloques ou plus, aller en page 2 puis cocher un tag | retour en page 1, un seul appel `GET /api/charms` dans l'onglet réseau. |
| 12 | Ouvrir `/chains` | « Plage (1) » seulement : les nombres sont ceux des chaînes. Cocher « Plage » donne `/chains?tags=plage` et la chaîne D. |
| 13 | API arrêtée sur `/api/tags` (ou bloquée dans l'onglet réseau) | message « Impossible de charger les tags… », les autres filtres et la liste fonctionnent. |
