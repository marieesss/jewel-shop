# Feature Specification: Filtrer le catalogue par tags dans l'interface

**Feature Branch**: `feat/104-filtre-catalogue-tags-front`

**Created**: 2026-11-12

**Status**: Draft

**Input**: Issue #104 « feat(catalog): filtrer le catalogue par tags dans l'interface ». Le filtrage et le nombre d'articles par tag sont fournis par l'API depuis #103.

## Clarifications

### Session 2026-11-12

- Q : le bouton « Effacer les filtres » efface-t-il seulement les tags, ou tous les filtres ? → R : seulement les tags. Il appartient au bloc des tags, et le bouton « Réinitialiser les filtres » existant reste inchangé.
- Q : décision 80, un tag dont le nombre d'articles vaut 0 est-il masqué ou grisé ? → R : masqué. Exception : un tag à 0 coché dans l'adresse reste affiché pour pouvoir être décoché.

## User Scenarios & Testing *(mandatory)*

### User Story 1 - Filtrer une liste en cochant des tags (Priority: P1)

Sur la liste des breloques ou des chaînes, le client voit les tags regroupés par catégorie (Thème, Couleur, Matière, Occasion), chacun suivi du nombre d'articles du type affiché qui le portent, par exemple « Mer (10) ». Il coche un ou plusieurs tags : la liste se recharge à la première page et ne montre que les articles correspondants. Les tags cochés dans une même catégorie s'additionnent (Mer OU Plage), les catégories se combinent entre elles (Mer OU Plage, ET Doré).

**Why this priority**: c'est le cœur de la fonctionnalité. Sans elle, les tags saisis par l'admin ne servent à rien côté client.

**Independent Test**: ouvrir `/charms`, cocher « Mer » et vérifier que seules les breloques portant « Mer » s'affichent, en page 1.

**Acceptance Scenarios**:

1. **Given** le catalogue des breloques, **When** le client coche « Mer », **Then** l'adresse de la page contient `?tags=mer` et la liste ne montre que les breloques portant « Mer ».
2. **Given** « Mer » coché, **When** le client coche aussi « Plage » (même catégorie), **Then** l'adresse contient les deux slugs et la liste montre les breloques portant « Mer » ou « Plage ».
3. **Given** « Mer » coché, **When** le client coche « Doré » (autre catégorie), **Then** la liste montre les breloques portant « Mer » et « Doré ».
4. **Given** le client est en page 3, **When** il coche ou décoche un tag, **Then** la liste revient en page 1.
5. **Given** « Mer » et « Doré » cochés, **When** le client décoche « Mer », **Then** `mer` disparaît de l'adresse et la liste est filtrée sur « Doré » seul.
6. **Given** le catalogue des chaînes, **When** le client affiche les filtres, **Then** les nombres affichés sont ceux des chaînes, et non des breloques.

---

### User Story 2 - Retrouver ses filtres depuis un lien partagé (Priority: P2)

Le client copie l'adresse d'une liste filtrée et l'envoie, ou la met en favori. À l'ouverture du lien, les mêmes tags sont cochés et la liste est déjà filtrée.

**Why this priority**: c'est la conservation des filtres dans l'URL demandée par la roadmap. Elle dépend de la story 1.

**Independent Test**: ouvrir directement `/charms?tags=mer,dore` et vérifier que « Mer » et « Doré » sont cochés et la liste filtrée.

**Acceptance Scenarios**:

1. **Given** l'adresse `/charms?tags=mer,dore`, **When** le client l'ouvre, **Then** « Mer » et « Doré » sont cochés et la liste est filtrée.
2. **Given** une liste filtrée, **When** le client utilise les boutons Précédent / Suivant du navigateur, **Then** les cases cochées et la liste suivent l'adresse affichée.
3. **Given** l'adresse `/charms?tags=mer,inconnu`, **When** le client l'ouvre, **Then** « Mer » est coché, le slug inconnu est ignoré, et aucune erreur n'est affichée.

---

### User Story 3 - Effacer les filtres (Priority: P3)

Le client qui a coché plusieurs tags revient à la liste complète en un clic avec le bouton « Effacer les filtres ».

**Why this priority**: confort d'usage. Le client peut aussi décocher les tags un par un.

**Independent Test**: cocher deux tags, cliquer « Effacer les filtres », vérifier que plus rien n'est coché et que `tags` a disparu de l'adresse.

**Acceptance Scenarios**:

1. **Given** des tags cochés, **When** le client clique « Effacer les filtres », **Then** `tags` disparaît de l'adresse, plus aucun tag n'est coché et la liste revient en page 1.
2. **Given** des tags cochés et une couleur choisie, **When** le client clique « Effacer les filtres », **Then** seuls les tags sont effacés : la couleur, le prix et le stock restent appliqués.
3. **Given** des tags cochés, **When** le client clique le bouton existant « Réinitialiser les filtres », **Then** son comportement actuel est inchangé (couleur, prix et stock remis à zéro).

---

### User Story 4 - Ne pas proposer de filtre menant à une liste vide (Priority: P3)

Un tag que ne porte aucun article du type affiché (nombre à 0) n'invite pas le client à le cocher.

**Why this priority**: la roadmap affiche le nombre pour éviter les listes vides. Le traitement exact dépend d'une décision ouverte.

**Independent Test**: créer un tag porté par aucune breloque, ouvrir `/charms` et vérifier son affichage.

**Acceptance Scenarios**:

1. **Given** un tag dont le nombre de breloques vaut 0, **When** le client affiche les filtres des breloques, **Then** le tag n'est pas affiché (décision 80 : masqué).
2. **Given** l'adresse `/charms?tags=xyz` où « xyz » a un nombre à 0, **When** le client l'ouvre, **Then** « xyz » est affiché coché pour qu'il puisse le décocher, et disparaît des filtres une fois décoché.

---

### Edge Cases

- Aucun tag n'existe pour le type affiché : la zone des tags n'est pas affichée, les autres filtres restent utilisables.
- Une catégorie ne contient aucun tag : elle n'est pas affichée.
- Le chargement des tags échoue : un message en français l'indique dans la zone des tags, la liste et les autres filtres continuent de fonctionner.
- L'adresse contient un tag dont le nombre vaut 0 (lien ancien) : par exception au masquage, il reste affiché coché et pris en compte dans le filtrage tant qu'il figure dans l'adresse, pour que le client puisse le décocher.
- Tous les tags d'une catégorie ont un nombre à 0 : la catégorie n'est pas affichée (sauf si l'un d'eux est coché).
- L'adresse contient `tags=` vide, des virgules en trop ou un slug en double : seuls les slugs non vides et distincts sont pris en compte.
- Le client passe des breloques aux chaînes par le menu : chaque liste a sa propre adresse, les tags cochés sur l'une ne s'appliquent pas à l'autre.
- Les tags cochés ne donnent aucun résultat (combinaison ET entre catégories) : le message « Aucun résultat » existant s'affiche.
- Le nombre affiché est global (décision de #103) : il ne change pas quand d'autres filtres sont cochés.

## Requirements *(mandatory)*

### Functional Requirements

- **FR-001**: Les listes des breloques et des chaînes DOIVENT afficher, dans le panneau de filtres existant, les tags du type affiché regroupés par catégorie, avec le libellé français de la catégorie (Thème, Couleur, Matière, Occasion), dans l'ordre fourni par l'API.
- **FR-002**: Chaque tag DOIT être présenté comme une case à cocher libellée « <nom> (<nombre>) », où le nombre est le nombre global d'articles du type affiché qui portent ce tag.
- **FR-003**: Cocher ou décocher un tag DOIT mettre à jour le paramètre `tags` de l'adresse (slugs séparés par des virgules), recharger la liste filtrée et revenir à la page 1.
- **FR-004**: Quand aucun tag n'est coché, le paramètre `tags` NE DOIT PAS figurer dans l'adresse.
- **FR-005**: À l'ouverture de la page, et à chaque changement de l'adresse (navigation Précédent / Suivant), les tags cochés et la liste DOIVENT refléter le paramètre `tags` de l'adresse.
- **FR-006**: Un slug de l'adresse qui ne correspond à aucun tag connu DOIT être ignoré sans message d'erreur.
- **FR-007**: Le bloc de filtres par tags DOIT porter son propre bouton « Effacer les filtres », qui retire `tags` de l'adresse, décoche tous les tags et revient à la page 1, sans toucher aux autres filtres. Le bouton existant « Réinitialiser les filtres » garde son comportement actuel.
- **FR-008**: Un tag dont le nombre vaut 0 NE DOIT PAS être affiché, sauf s'il est coché dans l'adresse : il reste alors affiché coché jusqu'à ce que le client le décoche.
- **FR-009**: Les filtres par tags DOIVENT se combiner avec les filtres existants (couleur, prix, stock) et la pagination.
- **FR-010**: Le bloc de filtres par tags DOIT être réutilisable tel quel par le parcours de création (#125) : il reçoit le type de produit et les tags cochés, et signale les changements, sans dépendre de la page catalogue.
- **FR-011**: Un échec du chargement des tags DOIT afficher un message en français, au vouvoiement, sans bloquer la liste.

### Key Entities

- **Tag filtrable** : tag proposé au client, avec son nom, son slug (identifiant dans l'adresse), sa catégorie et le nombre global d'articles du type affiché qui le portent.
- **Sélection de tags** : ensemble des slugs cochés, conservé dans l'adresse de la page sous la forme `?tags=slug1,slug2`.

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: Les quatre critères d'acceptation de l'issue #104 sont vérifiés à la main sur `/charms` et `/chains`.
- **SC-002**: Un lien copié depuis une liste filtrée, ouvert dans un nouvel onglet, affiche 100 % des mêmes tags cochés et la même liste.
- **SC-003**: Le client filtre la liste sur un tag en une seule action (un clic), sans validation supplémentaire.
- **SC-004**: Revenir à la liste complète demande au plus une action (« Effacer les filtres »).
- **SC-005**: Un échec du chargement des tags laisse la liste consultable dans 100 % des cas.

## Assumptions

- L'API de #103 est en production : filtre `tags` sur les listes et nombre global d'articles par tag pour un type de produit donné. Aucun changement d'API n'est nécessaire.
- Seul le filtre par tags est conservé dans l'adresse. Les filtres existants (couleur, prix, stock) et la page courante gardent leur comportement actuel, hors de l'adresse.
- La logique OU / ET entre tags est appliquée par l'API. L'interface se contente d'envoyer les slugs cochés.
- Le catalogue est accessible aux mêmes utilisateurs qu'aujourd'hui. La fonctionnalité est visible dès le merge (aucun masquage, l'issue la termine).
- Le parcours de création (#125) est hors périmètre : seul le caractère réutilisable du bloc est requis ici.
