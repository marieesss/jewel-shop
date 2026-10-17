# Roadmap mycharmly

Chaque version est un milestone GitHub. Chaque fonctionnalité ci-dessous sera découpée en issues de la taille d'une PR, avec DoR et DoD.

---

## v0.1 : Socle auth et accès

**Objectif** : l'application n'est accessible qu'aux personnes invitées, avec une authentification robuste.

### 1. Service d'email
- Interface `IEmailSender` côté back, avec une seule implémentation SMTP (`SmtpEmailSender`) : Mailpit en local (conteneur Docker), relais SMTP de Brevo en staging et en production. Envoi activé par `Email__Enabled`.
- Templates de mail stockés côté back.
- Envoi asynchrone via une table outbox et un `BackgroundService` : une action n'échoue pas si le fournisseur de mail est indisponible, le mail est renvoyé plus tard.

### 2. Politique de mot de passe
- 8 caractères minimum, au moins 1 majuscule, 1 chiffre et 1 caractère spécial.
- Validation côté back (bloquante) et côté front (règles cochées en direct pendant la saisie).

### 3. Refresh token
- Access token JWT de 15 min.
- Refresh token opaque, stocké hashé en base, envoyé en cookie `httpOnly`, `Secure`, `SameSite=Strict`.
- Rotation à chaque refresh ; si un token déjà utilisé est présenté, toute la session est révoquée.
- Révocation au logout et au reset du mot de passe.
- Front : intercepteur Axios (401 → refresh → rejoue la requête), avec mise en file des requêtes simultanées.

### 4. Protection des routes front
- `ProtectedRoute` : un utilisateur non connecté est redirigé vers `/login?returnUrl=...`, puis renvoyé sur la page demandée après connexion (corrige le bug `/chains`).
- `AdminRoute` : réservé au rôle Admin (les rôles Admin et User existent déjà).

### 5. Mot de passe oublié
- Token à usage unique, stocké hashé, valable 1 h, envoyé par mail.
- Réponse identique que l'email existe ou non.
- Un reset réussi révoque tous les refresh tokens de l'utilisateur.

### 6. Inscription sur invitation
- Table `Invitation` : email, token hashé, expiration, statut (`Pending`, `Accepted`, `Expired`, `Revoked`), `InvitedBy`, `CreatedAt`.
- L'admin saisit un email et un lien d'invitation est envoyé, valable **7 jours**.
- Inscription publique fermée : la création de compte exige une invitation valide, et l'email du compte est verrouillé sur celui de l'invitation.

### 7. Gestion des utilisateurs (admin)
- Nouvelle entrée « Utilisateurs » dans le menu admin.
- Liste des utilisateurs inscrits.
- Liste des invitations en attente, avec les actions renvoyer et révoquer.

---

## v0.2 : Création de collier et commandes

**Objectif** : un utilisateur compose son collier, passe commande, et l'admin suit la commande jusqu'à la réception. Pas de paiement en ligne dans cette version.

### 1. Modèle catalogue
- **Chaînes vendues au mètre** : `Chain` (nom, description, couleur, photo, lien, coût au mètre, prix au mètre, coût du fermoir, prix du fermoir, stock total en cm).
- Le stock est un total : l'admin saisit la longueur disponible en cm (par exemple 500), sans gérer de rouleaux séparés.
- Longueurs proposées au client : table de référence `ChainLength` (38, 42, 45, 50, 55, 60, 65, 70, 81 cm), administrable.
- Prix d'une chaîne = prix au mètre × longueur / 100 + prix du fermoir, arrondi à 2 décimales et calculé côté back.
- Une longueur n'est proposée que si le stock de la chaîne la couvre.
- Longueurs et stock stockés en **cm entiers** pour éviter les erreurs d'arrondi.
- Stock en unités sur les breloques.
- Formulaire admin de chaîne : le champ « Longueur (cm) » devient « Stock (cm) », « Coût / Prix » deviennent « Coût / Prix au mètre », et deux champs « Coût / Prix du fermoir » sont ajoutés.

### 2. Adresse de livraison
- Adresse enregistrée sur le profil utilisateur, préremplie au moment de la commande.
- L'adresse est **copiée** dans la commande, pour qu'un changement de profil ne modifie pas les anciennes commandes.

### 3. Parcours de création
- Bouton « Créer mon collier », puis un parcours en étapes :
  1. Choix de la chaîne (favoris en premier) et de la longueur, avec un guide visuel des longueurs.
  2. Choix des breloques sur une page dédiée (favoris en premier) : un clic ajoute une breloque, on peut la retirer, en mettre plusieurs identiques, et réordonner par drag and drop (dnd-kit).
- **Maximum 8 breloques**, vérifié côté front et côté back.
- État du collier conservé entre les étapes côté front. Pas de sauvegarde en base : une popup « Votre collier ne sera pas sauvegardé » s'affiche si l'utilisateur quitte le parcours.

### 4. Récapitulatif et commande
- Page récapitulative : chaque article avec son prix, et le prix total.
- Commentaire optionnel de l'utilisateur.
- Création de la commande : `Order` et `OrderLine`, avec les **prix copiés** au moment de la commande, et la position de chaque breloque.
- Stock décrémenté à la création de la commande (la longueur en cm pour la chaîne, une unité par breloque), recrédité en cas d'annulation.
- Mail de confirmation au client.

### 5. Suivi de commande (client)
- Onglet « Mes commandes », avec la liste et le détail.
- Statuts : `Validation` → `Traitement` → `Envoyé` → `Reçu`, plus `Annulée`.
- Le client marque la commande comme reçue (uniquement depuis `Envoyé`).
- Le client peut annuler uniquement en `Validation`.

### 6. Gestion des commandes (admin)
- Liste des commandes, filtrable par statut.
- Détail de la commande, notes internes (`OrderNote`), fiche client (infos, adresse, historique de commandes).
- Changement de statut selon une machine à états vérifiée côté back, avec historique (`OrderStatusHistory`).
- L'admin peut annuler une commande avant `Reçu`.
- Popup au changement de statut : case « Prévenir le client par mail » et message optionnel.

---

## v0.3 : Supervision et traitement des images

**Objectif** : surveiller la santé des services et automatiser la préparation des photos produit.

Contrainte : VPS OVH avec 2 vCores, 4 Go de RAM et 40 Go de disque, partagé avec une autre application.

### 1. Health checks et disponibilité
- Endpoints `/health` sur l'API .NET (avec vérification PostgreSQL) et sur l'API images.
- Uptime Kuma pour surveiller la disponibilité, avec alertes (mail ou Telegram).

### 2. Métriques et tableaux de bord
- OpenTelemetry côté .NET, `prometheus-flask-exporter` côté Flask.
- Grafana Cloud (offre gratuite) avec un agent léger sur le VPS, plutôt que Prometheus et Grafana auto-hébergés, pour économiser RAM et disque.

### 3. API images (Python Flask)
- Service interne, non exposé publiquement (réseau Docker interne et clé d'API).
- Paquets : Flask, Pillow, rembg avec onnxruntime, cloudinary, gunicorn.
- Traitement : suppression du fond, redimensionnement, export en PNG ou WebP transparent, puis upload sur Cloudinary. La compression à la livraison est assurée par Cloudinary (`f_auto`, `q_auto`).
- Modèle rembg chargé une seule fois au démarrage, un seul worker gunicorn pour ne pas dupliquer le modèle en RAM.

### 4. Intégration dans l'admin
- À l'upload d'une photo produit : l'API .NET appelle l'API images, puis stocke l'URL et le `public_id` Cloudinary.
- L'admin voit le résultat détouré et le valide, ou relance le traitement.

---

## Plus tard

- Paiement en ligne.

