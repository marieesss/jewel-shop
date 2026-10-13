# Contribuer à jewel-shop

Ce guide explique comment passer **d'une issue GitHub à une pull request mergée**.
Il s'adresse à toute personne qui contribue.

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

Chaque fonctionnalité vit dans `jewel-api/src/JewelryShop.API/Application/Features/<Fonctionnalité>/` :

```
Charms/
├── Charm.cs                 # entité
├── CharmRepository.cs       # interface + implémentation EF
├── CharmsController.cs      # endpoints HTTP, délègue à MediatR (Sender)
├── Commands/                # une commande = record + Validator + Handler dans le même fichier
├── Queries/
└── DTOs/                    # records + méthodes d'extension ToDto()
```

- **Un controller ne contient pas de logique métier** : il construit une commande ou une requête et l'envoie via `Sender`.
- **Toute commande a un validateur FluentValidation**. Le `ValidationBehavior` l'exécute automatiquement avant le handler.
- **Les erreurs métier passent par les exceptions dédiées** (`NotFoundException`, `ConflictException`, `ForbiddenAccessException`, `UnauthorizedException`, `ValidationException`). `ExceptionHandlingMiddleware` les convertit en codes HTTP 4xx. Ne renvoyez pas de `BadRequest()` à la main.
- La configuration EF de chaque entité va dans `Infrastructure/Persistence/Configurations/`. Un nouveau repository est enregistré dans `Startup.cs`.
- Les messages destinés à l'utilisateur sont **en français**.

### Organisation du front

- `src/api/` : appels HTTP (axios) et types des DTO. Ils doivent rester alignés sur les DTO de l'API.
- `src/stores/` : état MobX.
- `src/features/<domaine>/` : composants métier. `src/components/ui/` : composants génériques.
- `src/pages/` : pages routées (`admin/`, `user/`). `src/routes/guards.tsx` protège les routes.
- Les validations de formulaire (par exemple `productForm.ts`) **reproduisent** celles de FluentValidation côté back. Si vous modifiez une règle d'un côté, modifiez-la aussi de l'autre.

> **Avant d'écrire du code, trouvez la fonctionnalité existante la plus proche et reprenez sa structure** (nommage, découpage des fichiers, style). Par exemple, une nouvelle ressource administrable se calque sur `Charms` ou `Chains`.

---

## 2. Démarrer en local

```bash
cp .env.example .env        # puis remplir les valeurs (jamais commité)
docker compose up -d --build
```

- Application (passerelle Nginx) : http://127.0.0.1:8090
- pgAdmin : http://127.0.0.1:8092

Hors Docker :

```bash
cd jewel-api && dotnet build JewelryShop.sln && dotnet test JewelryShop.sln
cd jewel-front && npm ci && npm run build
```

---

## 3. D'une issue à la pull request : le déroulé

1. **Lire l'issue en entier** : besoin, critères d'acceptation, DoR et DoD (§4), issues liées.
   - S'il reste une question ouverte ou une règle ambiguë, **ne devinez pas**. Posez la question en commentaire de l'issue et attendez la réponse avant de coder la partie concernée.
2. **Créer une branche** depuis `main` à jour : `feature/<n°>-description-courte`, `fix/<n°>-…` ou `infra/<n°>-…`.
3. **Explorer le code** concerné et la fonctionnalité existante la plus proche (§1).
4. **Découper le travail en petites étapes** et faire **un commit par étape** (§7). Par exemple :
   1. la migration SQL ;
   2. l'entité et sa configuration EF ;
   3. la commande ou la requête, avec son validateur, son handler et **leurs tests** ;
   4. l'endpoint dans le controller ;
   5. les types et les appels API côté front ;
   6. l'interface (store, composants, page).

   Chaque commit compile et passe les tests. Ne faites pas un seul gros commit à la fin.
5. **Vérifier en local** avant de pousser :

   ```bash
   cd jewel-api && dotnet build JewelryShop.sln && dotnet test JewelryShop.sln
   cd jewel-front && npm run build
   ```

6. **Pousser la branche et ouvrir la pull request dès le dernier commit**, sans attendre (§7). La PR cible `main`.
7. **Suivre la CI** et corriger sur la même branche, avec de nouveaux commits, jusqu'à ce qu'elle soit verte.

Limites pour un agent :

- Ne poussez jamais directement sur `main` et ne mergez pas la PR vous-même : la relecture et le merge sont humains.
- Ne modifiez pas `deploy/`, `.github/workflows/` ni les secrets, sauf si l'issue le demande explicitement.
- Ne sortez pas du périmètre de l'issue. Une amélioration repérée en passant se signale en commentaire ou dans une nouvelle issue.

---

## 4. Definition of Ready / Definition of Done

### Definition of Ready : une issue peut être commencée quand…

- [ ] Le besoin est formulé du point de vue de l'utilisateur (client ou admin).
- [ ] Les critères d'acceptation sont écrits et testables (« étant donné… quand… alors… »).
- [ ] Les impacts sont identifiés : base de données (migration ?), API (DTO, endpoints), front, déploiement.
- [ ] Les règles métier sont tranchées. Aucune question ouverte ne reste dans l'issue.
- [ ] Les dépendances vers d'autres issues sont liées.

### Definition of Done : une issue est terminée quand…

- [ ] Tous les critères d'acceptation sont remplis.
- [ ] **Les tests unitaires ont été ajoutés ou mis à jour** pour couvrir le changement (§5).
- [ ] La CI est verte : build et tests de l'API, build du front (`tsc` et `vite build`).
- [ ] La migration SQL, si elle est nécessaire, est fournie et reprend les données existantes (§6).
- [ ] Les validations sont identiques côté back (FluentValidation) et côté front.
- [ ] La PR est relue et mergée sur `main`.
- [ ] Le déploiement en staging a réussi et les tests de fumée passent.
- [ ] La production est validée par une approbation manuelle dans GitHub.

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

- Un dossier par fonctionnalité, qui reprend `Application/Features` (`Auth/`, `Charms/`, …).
- Une classe `<ClasseTestée>Tests`.
- Les noms de tests sont des **phrases en français** qui décrivent le comportement : `Refuse_un_mot_de_passe_de_moins_de_8_caracteres`.
- Structure Arrange / Act / Assert, et un seul comportement par test. Utilisez `[Theory]` + `[InlineData]` pour les variantes d'un même cas.
- Pas d'accès réseau, pas de vraie base, pas de Cloudinary : remplacez les dépendances par des doublures (repositories, `IFileStorageService`, `IJwtService`, `IPasswordHasher`).

```bash
cd jewel-api && dotnet test JewelryShop.sln
```

Côté front, la CI ne fait aujourd'hui que vérifier les types et construire. Si une issue ajoute de la logique pure côté front (calcul, validation, formatage), ajoutez un lanceur de tests (Vitest), placez les tests à côté du fichier (`*.test.ts`) et branchez-les dans `ci.yml`.

---

## 6. Principes de conception et données

- **Montants et quantités** : stockez les grandeurs mesurables (longueurs, quantités, stocks) en **entiers dans la plus petite unité utile**, pour éviter les erreurs d'arrondi. Ne convertissez qu'à l'affichage et à la saisie.
- **Le back fait foi** : les prix et les calculs métier sont faits côté API. Le front affiche et pré-valide, mais ne décide pas.
- **Ce qui est enregistré reste figé** : une valeur copiée dans un document (par exemple le prix dans une commande) ne se recalcule pas si la donnée d'origine change ensuite.
- **Opérations liées, même transaction** : quand une action en entraîne d'autres (mouvement de stock, annulation…), toutes les écritures passent dans un seul `SaveChangesAsync`.

### Migrations

- Les migrations sont des fichiers SQL dans `jewel-api/db/migrations/`, nommés `V<n>__Description.sql` avec le numéro suivant.
- **Ne modifiez jamais une migration déjà mergée** : ajoutez-en une nouvelle.
- Une migration doit reprendre les données existantes. Quand vous supprimez ou transformez une colonne, indiquez dans la PR ce que deviennent les anciennes valeurs.
- Les migrations tournent dans un Job Kubernetes (`api-migrate`) **avant** le déploiement de l'API. En production, une sauvegarde est faite juste avant. Si une migration échoue, le déploiement s'arrête.

---

## 7. Commits et pull requests

### Commits : petits et fréquents

- **Commitez au fil de l'eau, une étape cohérente à la fois** (voir le découpage du §3). Une PR contient normalement plusieurs commits.
- Chaque commit compile et passe les tests. Il doit pouvoir être relu seul.
- Ne mélangez pas un refactoring ou un reformatage avec un changement de comportement dans le même commit.
- **Messages en français**, sur le modèle de l'historique : un titre qui dit *ce que fait* le changement, précisé si besoin après « : ».
  - ✅ `Sauvegardes Postgres en CronJob avec vérification et test de restauration`
  - ✅ `Déploiement continu sur Kubernetes : staging automatique, production validée`
  - ❌ `fix`, `wip`, `modifs`, `remove comment`

### Pull request : ouverte juste après le dernier commit

- Poussez la branche et **ouvrez la PR vers `main` dès que le dernier commit est fait**.
- Le titre suit les mêmes règles qu'un message de commit.
- La description contient :
  - `Closes #<n°>` ;
  - un résumé du changement et des choix faits ;
  - **Migration** : oui / non, et ce qu'elle fait aux données existantes ;
  - **Tests** : ce qui est couvert ;
  - **Vérification manuelle** : comment tester en local ou en staging.
- Les corrections demandées en relecture ou par la CI se font avec de nouveaux commits sur la même branche.
- Mettez à jour la documentation (ce fichier, les README, les commentaires des manifests) quand le fonctionnement change.

---

## 8. Intégration et déploiement continus

Sur chaque PR, la CI exécute :

1. **API** : `dotnet restore`, `build` en Release, puis `test`.
2. **Front** : `npm ci`, puis `npm run build` (vérification des types et build Vite).

Après le merge sur `main` :

3. publication des images sur `ghcr.io`, taguées avec le SHA du commit ;
4. déploiement en **staging**, suivi des tests de fumée (`/` et `/api/chains`) ;
5. déploiement en **production** après **approbation manuelle**, avec retour arrière automatique si les tests de fumée échouent.

Si vous ajoutez un endpoint critique, proposez de l'ajouter aux tests de fumée de `deploy-k8s.yml`.

---

## 9. Secrets et sécurité

- **Les secrets GitHub (environnements `staging` et `production`) font foi.** Ils sont poussés dans le cluster à chaque déploiement (`deploy/k8s/tools/apply-secrets.sh`).
- Ne commitez jamais `.env`, une clé, un kubeconfig ou un fichier `deploy/etat-des-lieux*.txt` (ils sont dans `.gitignore`).
- Un nouveau secret doit être ajouté à **trois endroits** : `.env.example` (sans valeur), les environnements GitHub, et `deploy-k8s.yml` / `apply-secrets.sh`.
- Moindre privilège : les workflows et le compte de service GitHub du cluster n'ont que les droits dont ils ont besoin. Les NetworkPolicies refusent par défaut tout flux qui n'est pas déclaré. Si un nouveau service doit communiquer avec un autre, ajoutez explicitement la règle correspondante.
