# Contribuer à mycharmly

Ce guide explique comment passer **d'une issue GitHub à une pull request mergée**.
Il s'adresse à toute personne qui contribue.

Documents liés :

- [`docs/ROADMAP.md`](docs/ROADMAP.md) : les versions prévues ;
- [`docs/architecture.md`](docs/architecture.md) : l'architecture actuelle et ses évolutions.

---

## 1. Le projet en bref

| Dossier | Contenu | Pile |
|---|---|---|
| `jewel-api/` | API REST | .NET 8, ASP.NET Core, MediatR, FluentValidation, EF Core + PostgreSQL, Evolve (migrations SQL) |
| `jewel-api/tests/` | Tests unitaires de l'API | xUnit |
| `jewel-api/db/migrations/` | Migrations SQL versionnées | `V<n>__Description.sql` |
| `jewel-front/` | Interface client et admin | React 18, TypeScript, Vite, MobX, Tailwind |
| `deploy/` | Infra : passerelle Nginx, k3s, manifests Kubernetes (base + overlays `staging` / `production`) | Kustomize |
| `.github/workflows/` | CI (`ci.yml`) et déploiement réutilisable (`deploy-k8s.yml`) | GitHub Actions |

### Organisation de l'API (par fonctionnalité)

Chaque fonctionnalité vit dans `jewel-api/src/JewelryShop.API/Features/<Fonctionnalité>/`. Le code partagé (`ApiControllerBase`, `BaseEntity`, `PagedResult`, exceptions, interfaces de services) vit dans `jewel-api/src/JewelryShop.API/Common/`.

```
Charms/
├── Entities/
│   └── Charm.cs             # entité
├── Repositories/
│   ├── ICharmRepository.cs  # interface
│   └── CharmRepository.cs   # implémentation EF
├── Controllers/
│   └── CharmsController.cs  # endpoints HTTP, délègue à MediatR (Sender) ; records de requête en bas du fichier
├── Commands/                # une commande = record + Validator + Handler dans le même fichier
├── Queries/                 # une requête = record + Handler
└── DTOs/                    # records + méthodes d'extension ToDto()
```

Nommage :

| Élément | Convention | Exemple |
|---|---|---|
| Dossier de fonctionnalité | PascalCase, pluriel | `Charms/`, `Favorites/` |
| Entité | PascalCase, singulier, hérite de `BaseEntity` | `Charm` |
| Commande / requête | verbe + nom + `Command` / `Query` | `CreateCharmCommand`, `GetCharmsQuery` |
| Validateur / handler | `<Commande>Validator`, `<Commande>Handler` | `CreateCharmCommandValidator` |
| Corps de requête HTTP | `<Action><Ressource>Request` | `CreateCharmRequest` |
| DTO de réponse | `<Ressource>Dto` | `CharmDto` |
| Repository | `I<Ressource>Repository` + `<Ressource>Repository`, un type par fichier | `ICharmRepository.cs`, `CharmRepository.cs` |
| Namespace | `JewelryShop.` + chemin du dossier | `JewelryShop.Features.Charms.Entities`, `JewelryShop.Common.Exceptions` |
| Route | `api/<ressource-au-pluriel>`, en kebab-case | `api/charms`, `api/admin/users` |
| Table et colonnes | snake_case, table au pluriel | `creation_items.creation_id` |

Règles :

- **Un controller ne contient pas de logique métier** : il construit une commande ou une requête et l'envoie via `Sender`.
- **Toute commande a un validateur FluentValidation**. Le `ValidationBehavior` l'exécute automatiquement avant le handler.
- **Chaque règle de validation porte un message français explicite** (`.WithMessage("…")`), identique au message du front. En filet de sécurité, la culture des messages de FluentValidation est fixée au français au démarrage (`ValidationCulture.UseFrench()`, appelé dans `Startup.AddApplication`) : une règle oubliée renvoie le message par défaut en français, et non en anglais comme le ferait la culture neutre du serveur. Ce filet ne dispense pas du `WithMessage`.
- **Les erreurs passent par les exceptions dédiées** (`NotFoundException`, `ConflictException`, `ForbiddenAccessException`, `UnauthorizedException`, `ValidationException`). `ExceptionHandlingMiddleware` les convertit en réponse JSON `{ status, title, errors }` avec le bon code HTTP. Ne renvoyez pas de `BadRequest()` à la main.
- La configuration EF de chaque entité va dans `Infrastructure/Persistence/Configurations/` (noms de tables et colonnes explicites, en snake_case). Un nouveau repository est enregistré dans `Startup.cs`.
- Les accès aux données passent par les repositories. Les lectures sont en `AsNoTracking()`, les listes sont paginées avec `PaginationDefaults.Normalize` et renvoient un `PagedResult<T>`.
- Les endpoints réservés à l'admin portent `[Authorize(Roles = "Admin")]`. L'utilisateur courant s'obtient avec `GetUserId()`, jamais depuis le corps de la requête.
- Les messages destinés à l'utilisateur sont **en français et au vouvoiement** (« Ce produit est déjà dans vos favoris. »).

### Organisation du front

- `src/api/` : appels HTTP (axios, client unique `api` de `client.ts`) et types des DTO. Ils doivent rester alignés sur les DTO de l'API. Une fonction par endpoint, documentée par sa route (`/** GET /api/charms/{id} → détail */`).
- `src/stores/` : état MobX (`makeAutoObservable`, mises à jour après `await` dans `runInAction`), exposé par les hooks de `stores/context.ts` (`useAuthStore()`…).
- `src/features/<domaine>/` : composants métier. `src/components/ui/` : composants génériques, sans appel API.
- `src/pages/` : pages routées (`admin/`, `user/`). `src/routes/guards.tsx` protège les routes. Les routes sont déclarées dans `App.tsx`.
- Les erreurs d'API s'affichent via `extractApiError()`. Le chargement de données simple passe par le hook `useAsync`.
- Composants en PascalCase (un composant exporté par fichier, nommé comme le fichier), hooks en `useXxx`, utilitaires en camelCase. Exports nommés, pas d'export par défaut (sauf `App`).
- Style avec Tailwind. La fonction `cn()` (`src/lib/cn.ts`) compose les classes.
- Les validations de formulaire (par exemple `productForm.ts`) **reproduisent** celles de FluentValidation côté back, avec les mêmes messages. Si vous modifiez une règle d'un côté, modifiez-la aussi de l'autre.

> **Avant d'écrire du code, trouvez la fonctionnalité existante la plus proche et reprenez sa structure** (nommage, découpage des fichiers, style). Par exemple, une nouvelle ressource administrable se calque sur `Charms` ou `Chains`.

### Lisibilité du code et commentaires

**C'est le code qui doit s'expliquer, pas les commentaires.**

- **Des noms qui disent l'intention** : `GetByIdWithItemsAsync`, `ExistsByEmailAsync`, et pas `Get2`, `data`, `tmp`, `flag`.
- **Des fonctions courtes qui font une seule chose.** Si un bloc a besoin d'un commentaire pour dire ce qu'il fait, extrayez-le dans une fonction dont le nom le dit.
- **Pas de nombres ni de chaînes magiques** : une constante ou une énumération nommée (`PaginationDefaults.MaxPageSize`).
- **Des conditions lisibles** : une condition complexe va dans une variable ou une propriété au nom explicite (`favorite.IsValid`).

Un commentaire explique **pourquoi**, jamais **quoi** : une contrainte extérieure (librairie, outil, infra), un choix qui paraît étrange, un piège, ou le contrat d'une interface publique quand la signature ne suffit pas.

```csharp
// ✅ Explique un choix.
// Les exceptions métier attendues (4xx) sont une réponse normale de l'API,
// pas un bug serveur : on les journalise sobrement, sans stacktrace.

// ❌ Paraphrase le code : à supprimer.
// Supprime l'ancienne image si elle existait
```

À ne pas faire : paraphraser le code, laisser du code commenté (git garde l'historique), signer ou dater un commentaire, laisser un `TODO` sans issue (écrire `// TODO(#42) : …`), laisser un commentaire devenu faux.

Les fichiers d'infra (`deploy/`, workflows, scripts) sont plus commentés que le code applicatif, parce que les options des outils sont rarement évidentes. La règle reste la même : expliquer le pourquoi, pas reformuler la commande.

---

## 2. Démarrer en local

Prérequis : Docker, SDK .NET 8 (ou plus récent), **Node.js 20** (version de la CI et de l'image Docker).

### Tout en Docker

```bash
cp .env.example .env        # puis remplir les valeurs (jamais commité)
docker compose up -d --build
```

- Application (passerelle Nginx) : http://127.0.0.1:8090
- pgAdmin : http://127.0.0.1:8092
- Mailpit (mails envoyés par l'API, rien ne part vers l'extérieur) : http://127.0.0.1:8025

### Hors Docker (développement)

```bash
# Base PostgreSQL seule, sur le port 5433 (et pgAdmin sur http://localhost:5050)
cd jewel-api && docker compose up -d postgres

# Mailpit : SMTP sur localhost:1025, mails consultables sur http://localhost:8025
cd jewel-api && docker compose up -d mailpit

# API : http://localhost:5030, Swagger sur /api/swagger. Migrations jouées au démarrage.
cd jewel-api && cp .env.example .env && dotnet run --project src/JewelryShop.API --launch-profile http

# Front : http://localhost:5173
cd jewel-front && npm ci && npm run dev
```

L'API lit sa configuration dans `jewel-api/.env` (modèle : `jewel-api/.env.example`, à compléter avec le mot de passe `postgres`, une clé JWT et les identifiants Cloudinary).

### Vérifications avant de pousser

```bash
# API : formatage (.editorconfig), build, tests
cd jewel-api && dotnet format JewelryShop.sln --verify-no-changes && dotnet build JewelryShop.sln && dotnet test JewelryShop.sln

# Front : lint (ESLint), formatage (Prettier), types et build, tests
cd jewel-front && npm run lint && npm run format:check && npm run build && npm test
```

Pour corriger le formatage automatiquement : `dotnet format JewelryShop.sln` dans `jewel-api`, `npm run format` dans `jewel-front`. Les règles sont dans `.editorconfig` (racine), `jewel-front/eslint.config.js` et `jewel-front/.prettierrc.json`. Un reformatage se fait dans un commit séparé, sans changement de comportement.

---

## 3. D'une issue à la pull request : le déroulé

1. **Lire l'issue en entier** : contexte, spécifications, critères d'acceptation, section « Impact production », DoR et DoD (§4), issues liées.
   - S'il reste une question ouverte ou une règle ambiguë, **ne devinez pas**. Posez la question en commentaire de l'issue et attendez la réponse avant de coder la partie concernée.
   - Une issue ne se commence que si elle porte le label `ready` et que ses dépendances sont fermées.
2. **Créer une branche** depuis `main` à jour, nommée selon le §7.
3. **Explorer le code** concerné et la fonctionnalité existante la plus proche (§1).
4. **Découper le travail en petites étapes** et faire **un commit par étape** (§7). Par exemple :
   1. la migration SQL ;
   2. l'entité et sa configuration EF ;
   3. la commande ou la requête, avec son validateur, son handler et **leurs tests** ;
   4. l'endpoint dans le controller ;
   5. les types et les appels API côté front ;
   6. l'interface (store, composants, page).

   Chaque commit compile et passe les tests. Ne faites pas un seul gros commit à la fin.
5. **Vérifier en local** avant de pousser (§2), puis tester manuellement le parcours concerné.
6. **Pousser la branche et ouvrir la pull request dès le dernier commit**, sans attendre (§7). La PR cible `main`.
7. **Suivre la CI** et corriger sur la même branche, avec de nouveaux commits, jusqu'à ce qu'elle soit verte.

Limites :

- Ne poussez jamais directement sur `main` et ne mergez pas votre propre PR sans relecture.
- Ne modifiez pas `deploy/`, `.github/workflows/` ni les secrets, sauf si l'issue le demande explicitement.
- Ne sortez pas du périmètre de l'issue. Une amélioration repérée en passant se signale en commentaire ou dans une nouvelle issue.

---

## 4. Definition of Ready / Definition of Done

Le modèle complet d'une issue (contexte, spécifications, critères d'acceptation, impact production, DoR, DoD, dépendances, estimation) est celui utilisé pour découper la [roadmap](docs/ROADMAP.md). Les points ci-dessous en sont le minimum.

### Definition of Ready : une issue peut être commencée quand…

- [ ] Les critères d'acceptation sont écrits et testables (« étant donné… quand… alors… »), y compris les cas d'erreur.
- [ ] Les dépendances sont terminées.
- [ ] Le contrat d'API est défini (si l'issue touche le back et le front).
- [ ] Le modèle de données est défini (si l'issue contient une migration).
- [ ] La taille est compatible avec une seule PR (environ une journée de travail).
- [ ] La section « Impact production » est remplie.
- [ ] Aucune question ouverte ne reste dans l'issue.

Le label `ready` est posé à la main après relecture de l'issue, jamais automatiquement.

### Definition of Done : une issue est terminée quand…

- [ ] Tous les critères d'acceptation sont remplis.
- [ ] Le code est conforme à ce guide, y compris les règles de lisibilité (§1) : pas de code commenté, pas de commentaire qui paraphrase le code ou devenu faux.
- [ ] **Les tests unitaires ont été ajoutés ou mis à jour** pour couvrir le changement (§5).
- [ ] La CI est verte (formatage, lint, build et tests).
- [ ] La migration SQL, si elle est nécessaire, est fournie, rétrocompatible et testée en local (§6).
- [ ] Les validations sont identiques côté back (FluentValidation) et côté front, et les erreurs sont gérées.
- [ ] Aucun secret ni donnée de configuration sensible n'est commité.
- [ ] La PR est déployable seule sans casser la production (§7) et sa section « Impact production » est à jour.
- [ ] La documentation est mise à jour si le comportement ou la configuration change.
- [ ] La PR contient `Closes #<n°>`.
- [ ] Le changement a été testé manuellement en local.
- [ ] La PR est relue et mergée sur `main`, le déploiement en staging a réussi et la production est validée.

---

## 5. Tests unitaires

> **Toute modification de comportement s'accompagne de tests unitaires ajoutés ou mis à jour.**
> Si un test existant ne correspond plus à la nouvelle règle métier, **corrigez le test**. Ne le supprimez pas et ne le désactivez pas pour faire passer la CI.

Ce qu'il faut tester :

- **Validateurs FluentValidation** : un cas valide, puis un cas invalide par règle.
- **Handlers MediatR** : le cas nominal, les cas d'erreur (`NotFoundException`, `ConflictException`, `ForbiddenAccessException`…) et les effets attendus (sauvegarde appelée, valeurs transformées).
- **Calculs métier** (prix, quantités, disponibilité, règles de stock) : le cas nominal et les **cas limites** (zéro, valeur exactement égale au seuil, valeur hors bornes).
- **Corrections de bug** : écrivez d'abord un test qui reproduit le bug, puis corrigez le code.

Pour que le code reste testable, placez les calculs métier dans des **fonctions pures ou des méthodes du domaine** plutôt que dans les controllers ou les requêtes EF.

Conventions (voir `jewel-api/tests/JewelryShop.API.Tests/`) :

- Un dossier par fonctionnalité, qui reprend `Features/` (`Auth/`, `Charms/`, …).
- Une classe `<ClasseTestée>Tests`.
- Les noms de tests sont des **phrases en français** qui décrivent le comportement : `Refuse_un_mot_de_passe_de_moins_de_8_caracteres`.
- Structure Arrange / Act / Assert, et un seul comportement par test. Utilisez `[Theory]` + `[InlineData]` pour les variantes d'un même cas.
- Pas d'accès réseau, pas de vraie base, pas de Cloudinary : remplacez les dépendances par des doublures (repositories, `IFileStorageService`, `IJwtService`, `IPasswordHasher`).

```bash
cd jewel-api && dotnet test JewelryShop.sln
```

Côté front, la logique pure (calcul, validation, formatage) est testée avec Vitest. Les tests sont placés à côté du fichier testé (`*.test.ts`), avec la même convention de noms en français, et la CI les lance avec `npm test`.

---

## 6. Données et migrations

### Principes

- **Montants et quantités** : stockez les grandeurs mesurables (longueurs, quantités, stocks) en **entiers dans la plus petite unité utile** (par exemple des cm), pour éviter les erreurs d'arrondi. Ne convertissez qu'à l'affichage et à la saisie. Les prix restent en `numeric(10,2)` / `decimal`.
- **Le back fait foi** : les prix et les calculs métier sont faits côté API. Le front affiche et pré-valide, mais ne décide pas.
- **Ce qui est enregistré reste figé** : une valeur copiée dans un document (par exemple le prix dans une commande) ne se recalcule pas si la donnée d'origine change ensuite.
- **Opérations liées, même transaction** : quand une action en entraîne d'autres (mouvement de stock, annulation…), toutes les écritures passent dans un seul `SaveChangesAsync`.

### Migrations SQL (Evolve)

Le schéma est géré par des **migrations SQL Evolve**, pas par les migrations EF Core. EF Core sert seulement d'ORM : sa configuration (`Configurations/`) doit correspondre exactement au schéma créé par les migrations.

- Les fichiers sont dans `jewel-api/db/migrations/`, nommés `V<n>__Description.sql` avec le numéro suivant (`V2__Add_invitations.sql`).
- **Ne modifiez jamais une migration déjà mergée** : ajoutez-en une nouvelle. Evolve vérifie les sommes de contrôle et refuse de démarrer si un fichier appliqué a changé.
- En local, les migrations sont jouées au démarrage de l'API. En staging et en production, elles tournent dans un Job Kubernetes (`api-migrate`) **avant** le déploiement de l'API. En production, une sauvegarde est faite juste avant. Si une migration échoue, le déploiement s'arrête.
- Testez chaque migration en local sur une base qui contient des données, pas seulement sur une base vide.

### Migrations rétrocompatibles

La migration est appliquée **pendant que l'ancienne version de l'API tourne encore**. Et si le déploiement échoue, le retour arrière remet l'ancienne API, **mais pas l'ancien schéma**. Toute migration doit donc fonctionner avec la version précédente du code.

On applique le principe **expand / contract** : on ajoute avant de supprimer.

- Une colonne ajoutée à une table existante est **nullable ou a une valeur par défaut**.
- **Pas de suppression ni de renommage** de colonne ou de table dans la même PR que le code qui arrête de l'utiliser. Le déroulé est en trois PR : ajouter la nouvelle colonne et écrire dans les deux ; basculer la lecture ; supprimer l'ancienne colonne dans une issue séparée.
- Pas de nouvelle contrainte `NOT NULL`, `UNIQUE` ou `CHECK` sur des données existantes sans reprendre ces données dans la même migration.
- Toute migration qui touche des données existantes décrit, dans l'issue et dans la PR, **ce que deviennent les lignes déjà en base**.

---

## 7. Commits, branches et pull requests

### Branches

Format : `<type>/<n°-issue>-<slug>`, en minuscules, mots séparés par des tirets, sans accents.

- `feat/12-rotation-refresh-token`
- `fix/27-redirection-apres-login`

Les types sont ceux des commits (`feat`, `fix`, `chore`, `refactor`, `test`, `docs`). Un travail sans issue (rare, par exemple de la documentation) omet le numéro : `docs/conventions`.

### Commits : Conventional Commits, en français

Format : `<type>(<scope>): <description à l'infinitif>`

- `feat(auth): ajouter la rotation des refresh tokens`
- `fix(catalog): refuser une longueur supérieure au stock`
- `docs: ajouter le guide d'architecture`

| Type | Usage |
|---|---|
| `feat` | nouvelle fonctionnalité |
| `fix` | correction de bug |
| `refactor` | changement de code sans changement de comportement |
| `test` | ajout ou correction de tests uniquement |
| `docs` | documentation uniquement |
| `chore` | outillage, dépendances, CI, infra, nettoyage |

Scopes : `auth`, `email`, `users`, `catalog`, `creation`, `order`, `admin`, `favorites`, `infra`, `ci`, `monitoring`, `image-api`. Le scope peut être omis pour un changement transverse (`docs: …`). Un nouveau scope s'ajoute à cette liste dans la même PR.

Règles :

- Description en minuscules, à l'infinitif, sans point final, 72 caractères maximum pour la première ligne.
- Un changement cassant est signalé par `!` (`feat(auth)!: …`) et un pied de page `BREAKING CHANGE: …`. Il doit rester exceptionnel (voir « Ne pas casser la production »).
- **Commitez au fil de l'eau, une étape cohérente à la fois** (voir le découpage du §3). Une PR contient normalement plusieurs commits.
- Chaque commit compile et passe les tests. Il doit pouvoir être relu seul.
- Ne mélangez pas un refactoring ou un reformatage avec un changement de comportement dans le même commit.

### Pull request : ouverte juste après le dernier commit

- Poussez la branche et **ouvrez la PR vers `main` dès que le dernier commit est fait**.
- Le titre suit le format des commits.
- La description contient :
  - un **résumé** du changement et des choix faits ;
  - `Closes #<n°>` ;
  - une section **Impact production** : migration, API, visibilité, configuration, retour arrière (voir ci-dessous) ;
  - **Comment tester** : en local ou en staging ;
  - la **checklist de la DoD** de l'issue, cochée.
- Les corrections demandées en relecture ou par la CI se font avec de nouveaux commits sur la même branche.
- Mettez à jour la documentation (ce fichier, `docs/`, les README, les commentaires des manifests) quand le fonctionnement change.

### Ne pas casser la production

**Chaque PR doit pouvoir être mergée et déployée seule, sans casser ce qui tourne.** Le merge sur `main` déclenche le déploiement en staging, puis en production après validation.

- **Migrations rétrocompatibles** : voir §6.
- **API rétrocompatible** : pas de changement cassant sur un endpoint utilisé par le front en production (suppression ou renommage d'un champ, d'une route, changement de type ou de code de retour). Si c'est inévitable : créer le nouvel endpoint, migrer le front, puis supprimer l'ancien dans une issue séparée. Ajouter un champ optionnel n'est pas cassant.
- **Fonctionnalité incomplète invisible** : une fonctionnalité répartie sur plusieurs issues reste masquée jusqu'à l'issue qui la termine (route front non déclarée ou absente du menu, endpoint désactivé par une option de configuration `Features__<Nom>` à `false` par défaut).
- **Configuration** : toute nouvelle variable d'environnement a un **comportement par défaut sûr** si elle est absente, et elle est documentée (§9).
- **Retour arrière** : chaque PR indique comment l'annuler. Pour le code, `kubectl rollout undo` ou un revert. Pour les données, la sauvegarde faite avant la migration de production.

Section « Impact production » d'une PR :

```markdown
## Impact production
- Migration : aucune / rétrocompatible (expliquer) / effet sur les données existantes
- API : aucun changement / ajout / changement cassant (interdit sans validation)
- Visibilité : visible dès le merge / masquée jusqu'à #...
- Configuration : nouvelles variables d'environnement et valeur par défaut
- Retour arrière : comment annuler si le déploiement pose problème
```

---

## 8. Intégration et déploiement continus

Sur chaque PR, la CI exécute :

1. **API** : `dotnet restore`, vérification du formatage (`dotnet format --verify-no-changes`), `build` en Release, puis `test`.
2. **Front** : `npm ci`, `npm run lint`, `npm run format:check`, `npm run build` (vérification des types et build Vite), puis `npm test` (Vitest).

Ces deux jobs, `API (.NET)` et `Front (React)`, sont obligatoires pour merger sur `main`.

Après le merge sur `main` :

3. publication des images sur `ghcr.io`, taguées avec le SHA du commit ;
4. déploiement en **staging**, suivi des tests de fumée (`/` et `/api/chains`) ;
5. déploiement en **production** après **approbation manuelle**, avec retour arrière automatique si les tests de fumée échouent.

Si vous ajoutez un endpoint critique, proposez de l'ajouter aux tests de fumée de `deploy-k8s.yml`.

---

## 9. Secrets et sécurité

- **Les secrets GitHub (environnements `staging` et `production`) font foi.** Ils sont poussés dans le cluster à chaque déploiement (`deploy/k8s/tools/apply-secrets.sh`).
- Ne commitez jamais `.env`, une clé, un kubeconfig ou un fichier `deploy/etat-des-lieux*.txt` (ils sont dans `.gitignore`).
- Un nouveau secret doit être ajouté à **trois endroits** : `.env.example` (sans valeur), les environnements GitHub, et `deploy-k8s.yml` / `apply-secrets.sh`. Une nouvelle variable non secrète va dans `deploy/k8s/base/api-config.yaml` et dans `compose.yaml`.
- Moindre privilège : les workflows et le compte de service GitHub du cluster n'ont que les droits dont ils ont besoin. Les NetworkPolicies refusent par défaut tout flux qui n'est pas déclaré. Si un nouveau service doit communiquer avec un autre, ajoutez explicitement la règle correspondante.
