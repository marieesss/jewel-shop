#!/usr/bin/env bash
# Étape 7a — Installation de k3s avec Traefik et ServiceLB sur les ports 80/443.
# Nginx de l'hôte est ARRÊTÉ : Traefik devient la porte d'entrée du serveur.
# mycharmly.eu est coupé jusqu'à la fin de l'étape 7 (choix assumé).
# Le Compose jewel-app reste démarré (sans accès public) pour récupérer ses données en 7e.
#
# À lancer par marie avec sudo, depuis le dossier contenant ce script et config.yaml :
#
#   sudo bash 07a-installer-k3s.sh
#
# Retour arrière complet si besoin :
#   sudo /usr/local/bin/k3s-uninstall.sh
#   sudo systemctl enable --now nginx certbot.timer

set -euo pipefail

K3S_VERSION="v1.36.5+k3s1"   # version épinglée : la même à chaque installation
HERE="$(cd "$(dirname "$0")" && pwd)"
KUBECTL="k3s kubectl"

step() { printf '\n\033[1;34m==> %s\033[0m\n' "$1"; }
ok()   { printf '    \033[32mOK\033[0m %s\n' "$1"; }
warn() { printf '    \033[33mATTENTION\033[0m %s\n' "$1"; }
die()  { printf '    \033[31mERREUR\033[0m %s\n' "$1" >&2; exit 1; }

[ "$(id -u)" -eq 0 ] || die "lancer avec sudo"
[ -f "$HERE/config.yaml" ] || die "config.yaml manquant à côté du script"

# ── 1. Vérifications : on n'installe rien si une condition n'est pas remplie ──
step "1. Vérifications préalables"
if command -v k3s >/dev/null; then die "k3s est déjà installé ($(k3s --version | head -1))"; fi

AVAIL_MB=$(free -m | awk '/^Mem:/ {print $7}')
[ "$AVAIL_MB" -ge 1200 ] || die "seulement ${AVAIL_MB} Mo de RAM disponibles (1200 minimum)"
ok "${AVAIL_MB} Mo de RAM disponibles"

for port in 6443 10250 10257 10259; do
  if ss -tlnH "sport = :$port" | grep -q .; then die "le port $port est déjà utilisé"; fi
done
ok "ports 6443, 10250, 10257, 10259 libres"

if ip route | grep -qE '^10\.4[23]\.'; then die "un réseau 10.42/10.43 existe déjà sur la machine"; fi
ok "pas de conflit avec les réseaux 10.42.0.0/16 et 10.43.0.0/16 (Docker utilise 172.x)"

# ── 2. Sauvegardes avant de changer de porte d'entrée ─────────────────────────
step "2. Sauvegardes"
# timeout : si Postgres ne répond pas, on s'arrête au bout de 5 min au lieu de bloquer,
# AVANT d'avoir touché à Nginx (le site reste en ligne).
timeout 300 sudo -u deploy /srv/jewel-app/scripts/backup.sh pre-k3s | sed 's/^/    /' \
  || die "sauvegarde impossible (bloquée ou en erreur) : rien n'a été modifié"
install -d -m 700 /root/backups
BACKUP="/root/backups/nginx-$(date +%Y%m%d-%H%M%S).tar.gz"
tar -czf "$BACKUP" -C / etc/nginx
ok "configuration Nginx : $BACKUP"

# ── 3. Libérer les ports 80/443 ───────────────────────────────────────────────
step "3. Arrêt de Nginx (la configuration reste en place pour un retour arrière)"
# disable : ne redémarre pas au prochain boot. Les fichiers de /etc/nginx ne sont pas touchés.
systemctl disable --now nginx
# certbot renouvelait les certificats via Nginx : sans Nginx il échouerait chaque jour.
# Traefik gérera les certificats (étape 7e).
systemctl disable --now certbot.timer 2>/dev/null || true
for port in 80 443; do
  if ss -tlnH "sport = :$port" | grep -q .; then die "le port $port est encore utilisé après l'arrêt de Nginx"; fi
done
ok "ports 80 et 443 libres"

# ── 4. Configuration de k3s ───────────────────────────────────────────────────
step "4. Configuration /etc/rancher/k3s/config.yaml"
install -d -m 755 /etc/rancher/k3s
install -m 600 "$HERE/config.yaml" /etc/rancher/k3s/config.yaml
ok "installée"

# ── 5. Pare-feu : laisser les pods parler au nœud ─────────────────────────────
step "5. Règles ufw pour les réseaux internes du cluster"
# Les pods (ex. CoreDNS) doivent joindre l'API du cluster sur l'hôte. ufw refusant
# tout trafic entrant par défaut, on autorise UNIQUEMENT les adresses internes du cluster.
# Ces adresses n'existent pas sur Internet : rien n'est ouvert vers l'extérieur.
ufw allow from 10.42.0.0/16 to any comment 'k3s pods' >/dev/null
ufw allow from 10.43.0.0/16 to any comment 'k3s services' >/dev/null
ok "10.42.0.0/16 et 10.43.0.0/16 autorisés (80/443 déjà ouverts depuis l'étape 0)"

# ── 6. Installation ───────────────────────────────────────────────────────────
step "6. Installation de k3s $K3S_VERSION"
# Le script officiel est téléchargé dans un fichier (lisible avant exécution),
# puis lancé avec la version épinglée.
curl -sfL https://get.k3s.io -o /tmp/k3s-install.sh
INSTALL_K3S_VERSION="$K3S_VERSION" sh /tmp/k3s-install.sh
rm -f /tmp/k3s-install.sh

# ── 7. Attente du cluster ─────────────────────────────────────────────────────
step "7. Attente du nœud, des composants système et de Traefik"
until $KUBECTL get node mycharmly-node-1 >/dev/null 2>&1; do sleep 2; done
$KUBECTL wait --for=condition=Ready node/mycharmly-node-1 --timeout=180s
# Traefik est installé par un Job Helm : son Deployment n'existe qu'après quelques secondes.
until $KUBECTL -n kube-system get deployment traefik >/dev/null 2>&1; do sleep 3; done
$KUBECTL -n kube-system rollout status deployment/traefik --timeout=300s
# Les pods "tâche terminée" (Succeeded, ex. le Job Helm) ne sont jamais Ready : exclus.
$KUBECTL -n kube-system wait --for=condition=Ready pod --all \
  --field-selector=status.phase!=Succeeded --timeout=300s
ok "cluster prêt"

# ── 8. Contrôles ──────────────────────────────────────────────────────────────
step "8. Contrôles"
NODE_IP=$(hostname -I | awk '{print $1}')
# 404 attendu : Traefik répond, mais aucune route n'est encore définie.
CODE=$(curl -s -o /dev/null -m 5 -w '%{http_code}' "http://$NODE_IP/" || true)
[ "$CODE" = "404" ] && ok "Traefik répond sur le port 80 (404 normal : aucune route)" \
  || warn "le port 80 a répondu '$CODE' au lieu de 404"
CODE=$(curl -sk -o /dev/null -m 5 -w '%{http_code}' "https://$NODE_IP/" || true)
[ "$CODE" = "404" ] && ok "Traefik répond sur le port 443 (certificat provisoire auto-signé)" \
  || warn "le port 443 a répondu '$CODE' au lieu de 404"
docker compose -f /srv/jewel-app/compose.yaml ps --format '    {{.Service}}: {{.Status}}' 2>/dev/null \
  || warn "impossible de lire l'état du Compose jewel-app"
free -m | awk '/^Mem:/ {printf "    RAM disponible après installation : %s Mo\n", $7}'

step "Terminé"
$KUBECTL get nodes -o wide
