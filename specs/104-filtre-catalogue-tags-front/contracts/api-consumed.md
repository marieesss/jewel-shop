# Contrat : endpoints consommés (inchangés, livrés par #103)

Aucun endpoint n'est ajouté ni modifié. Référence : `docs/architecture.md` §2, puces « Tags » et « Filtre du catalogue par tags ».

## `GET /api/tags?kind=chain|charm` (anonyme)

- 200 : `[{ id, name, slug, category, count }]`, triés par catégorie (`Theme`, `Color`, `Material`, `Occasion`) puis par nom.
- `count` : nombre **global** de chaînes (`kind=chain`) ou de breloques (`kind=charm`) qui portent le tag, indépendant des filtres.
- 400 « Type d'article invalide. » sous `Kind` pour une autre valeur. Le front n'envoie que `chain` ou `charm`.

## `GET /api/charms?tags=…` et `GET /api/chains?tags=…`

- `tags` : slugs séparés par des virgules (`mer,plage,dore`), facultatif.
- OU à l'intérieur d'une catégorie, ET entre catégories. Un slug inconnu est ignoré ; si aucun n'est connu, la liste n'est pas filtrée.
- Se combine avec `color`, `minPrice`, `maxPrice`, `inStockOnly`, `page` et `pageSize`. `totalCount` ne compte que les articles filtrés.
