# Constitution de mycharmly

Ce document fixe les principes non négociables du projet. Il ne remplace pas les guides : les
règles détaillées vivent dans [`CONTRIBUTING.md`](../../CONTRIBUTING.md) et
[`docs/architecture.md`](../../docs/architecture.md), dont la lecture est obligatoire avant toute
spec, tout plan et toute implémentation. En cas d'écart entre ces guides et une spec, ce sont les
guides qui s'appliquent.

## Principes fondamentaux

### I. L'issue est la source du besoin

- Une spec part d'une issue GitHub qui porte le label `ready`, dont la Definition of Ready est
  remplie et dont les dépendances sont fermées.
- Le dossier de spec se nomme `specs/<n°-issue>-<slug>` (exemple : `specs/99-fusionner-deux-tags`).
  Le numéro est celui de l'issue, jamais un numéro séquentiel.
- Une question ouverte ou une règle ambiguë se marque `[NEEDS CLARIFICATION]` et se pose en
  commentaire de l'issue. Elle n'est jamais tranchée par supposition.

Raison : l'issue est relue et validée à la main. La spec la précise, elle ne la contredit pas.

### II. On se calque sur l'existant

- L'API suit l'organisation par fonctionnalité (`Features/<Fonctionnalité>/` avec Entities,
  Repositories, Controllers, Commands, Queries, Validators, DTOs) sur .NET 8, MediatR,
  FluentValidation et EF Core. Les accès aux données passent par les repositories.
- Le front suit son organisation (`api/`, `stores/` MobX, `features/`, `components/ui/`, `pages/`)
  sur React, TypeScript et Tailwind.
- Chaque plan nomme la fonctionnalité existante la plus proche et reprend sa structure, son
  nommage et son style. Toute nouvelle dépendance ou tout nouveau motif d'architecture est justifié
  dans le plan.

Raison : un code homogène se relit vite et se maintient seul.

### III. Une validation, deux côtés

- Toute commande a un validateur FluentValidation, et chaque règle porte un message français
  explicite (`.WithMessage("…")`).
- Le formulaire front qui envoie cette commande reproduit les mêmes règles avec les mêmes messages.
  Modifier une règle d'un côté impose de la modifier de l'autre dans la même PR.
- Les erreurs passent par les exceptions dédiées (`NotFoundException`, `ConflictException`, etc.),
  jamais par un `BadRequest()` écrit à la main.

Raison : l'utilisateur voit le même message, que l'erreur soit détectée par le front ou par l'API.

### IV. Rien ne casse la production

- L'API reste rétrocompatible : aucun champ, route, type ou code de retour utilisé par le front en
  production n'est supprimé ni renommé. Un changement inévitable passe par un nouvel endpoint.
- Les migrations suivent le principe expand / contract : colonne ajoutée nullable ou avec une valeur
  par défaut, aucune suppression ni aucun renommage dans la PR qui arrête de l'utiliser, et le sort
  des lignes existantes est décrit.
- Une fonctionnalité répartie sur plusieurs issues reste masquée (route non déclarée ou option
  `Features__<Nom>` à `false`) jusqu'à l'issue qui la termine.
- Aucun secret, clé ou fichier `.env` n'est commité.

Raison : chaque merge sur `main` est déployé en staging puis en production, et le retour arrière
ne restaure pas le schéma.

### V. Chaque étape est testée

- Les tâches sont découpées pour qu'un commit corresponde à une étape cohérente qui compile et passe
  les tests.
- Les tests unitaires sont ajoutés ou mis à jour pour couvrir le changement, selon le §5 de
  `CONTRIBUTING.md`. Les tests d'intégration (Testcontainers PostgreSQL) continuent de passer.
- Les vérifications de `CONTRIBUTING.md` (build, tests, lint, format) passent avant l'ouverture de
  la PR.

Raison : un commit doit pouvoir être relu seul, et la CI ne doit révéler aucune surprise.

### VI. Le code s'explique

- Les noms disent l'intention, les fonctions sont courtes, il n'y a ni nombre ni chaîne magique.
- Un commentaire explique pourquoi, jamais quoi. Pas de code commenté, et tout `TODO` cite son
  issue (`// TODO(#42) : …`).

Raison : le commentaire qui paraphrase le code finit toujours par devenir faux.

### VII. Le français, partout où on lit

- Les messages destinés à l'utilisateur sont en français, au vouvoiement.
- Les specs, plans, tâches, checklists, commits et PR sont rédigés en français.

Raison : un seul public, une seule langue.

### VIII. Le périmètre de l'issue, rien de plus

- Une spec, un plan ou une tâche ne dépasse pas le périmètre de l'issue. Une amélioration repérée
  en passant devient une nouvelle issue.
- `deploy/`, `.github/workflows/` et les secrets ne sont modifiés que si l'issue le demande
  explicitement.

Raison : une PR ciblée se relit et se revert facilement.

## Contraintes techniques

- API : .NET 8, ASP.NET Core, MediatR, FluentValidation, EF Core + PostgreSQL, migrations SQL
  Evolve (`V<n>__Description.sql`, jamais modifiées une fois mergées).
- Front : React 18, TypeScript, Vite, MobX, Tailwind, Node.js 20.
- Infra : Nginx, k3s, Kustomize (overlays `staging` et `production`), GitHub Actions.
- Une technologie absente de cette liste ne s'introduit que par une issue dédiée.

## Déroulé et contrôles qualité

1. Spec (`spec.md`) écrite depuis l'issue, sans choix technique, puis clarifiée jusqu'à ce qu'il ne
   reste aucun `[NEEDS CLARIFICATION]`.
2. Plan (`plan.md`, contrat d'API et modèle de données si besoin) vérifié principe par principe
   contre cette constitution. Un écart est justifié dans le plan ou corrigé.
3. Tâches (`tasks.md`) ordonnées, chacune correspondant à un commit.
4. Implémentation sur une branche `<type>/<n°-issue>-<slug>`, commits Conventional Commits.
5. PR vers `main` avec la checklist de la Definition of Done. Le dossier `specs/…` fait partie de la
   PR.

## Gouvernance

- Cette constitution prime sur les specs, plans et tâches. Les guides `CONTRIBUTING.md` et
  `docs/architecture.md` en détaillent l'application.
- Toute modification passe par une PR relue, qui met à jour la version et la date d'amendement.
- Version sémantique : MAJEURE pour un principe supprimé ou redéfini, MINEURE pour un principe ou
  une section ajoutée ou nettement élargie, CORRECTIVE pour une reformulation sans changement de
  sens.
- Chaque relecture de PR vérifie le respect des principes. Une complexité supplémentaire doit être
  justifiée dans le plan.

**Version** : 1.0.0 | **Ratifiée le** : 2026-11-12 | **Dernier amendement** : 2026-11-12
