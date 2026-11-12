# Modèle de données : filtrer le catalogue par tags dans l'interface

Aucune donnée persistée ni aucune migration. Ce document décrit les types front et l'état de la page.

## TagCountDto (`api/tags.ts`, nouveau)

Réponse de `GET /api/tags?kind=chain|charm` (miroir de `TagCountDto` côté API).

| Champ | Type | Description |
|---|---|---|
| `id` | `number` | identifiant du tag |
| `name` | `string` | nom affiché (« Mer ») |
| `slug` | `string` | identifiant dans l'adresse (« mer ») |
| `category` | `string` | `Theme`, `Color`, `Material` ou `Occasion` |
| `count` | `number` | nombre global d'articles du type demandé qui portent le tag (≥ 0) |

Déclaré comme `interface TagCountDto extends TagDto { count: number }`.

## TagCountKind (`api/tags.ts`, nouveau)

`'chain' | 'charm'`, valeur du paramètre `kind`. Mêmes valeurs que `ProductKind`.

## ProductListParams (`api/products.ts`, modifié)

Ajout de `tags?: string` : slugs séparés par des virgules, absent quand aucun tag n'est coché.

## Sélection de tags (état de la page)

- **Source** : paramètre d'adresse `tags` (voir [contracts/tag-filters-ui.md](contracts/tag-filters-ui.md)).
- **Forme lue** : `string[]` de slugs, sans valeur vide ni doublon, dans l'ordre de l'adresse (`parseTagSlugs`).
- **Règles** :
  - cocher ajoute le slug à la fin, décocher le retire (`toggleTagSlug`) ;
  - liste vide : `tags` est retiré de l'adresse (`withTagSlugs`) ;
  - un slug inconnu est conservé tel quel et ignoré par l'API.

## Page courante (état local de la page)

`{ page: number; tagsParam: string | null }`. La page affichée vaut `page` si `tagsParam` égale le paramètre `tags` actuel, sinon 1 (`currentPage`). Changer de page enregistre `{ page, tagsParam: paramètre actuel }`.

## Groupe de tags visible (dérivé)

`{ category: TagCategory; label: string; tags: TagCountDto[] }`, produit par `visibleTagGroups(tags, selectedSlugs)` :

- ordre des catégories : `TAG_CATEGORIES` ; ordre des tags : celui de l'API ;
- un tag est visible si `count > 0` ou si son slug est coché ;
- une catégorie sans tag visible est absente ;
- un tag d'une catégorie inconnue n'est pas affiché.
