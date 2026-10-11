#!/usr/bin/env bash
# Étape 8b — Accès de GitHub Actions au cluster, avec le moindre privilège.
# À lancer par marie avec sudo, depuis ~/k8s (après "scp -r deploy/k8s ...") :
#
#   sudo bash tools/08b-acces-github.sh ~/jewel_tunnel.pub
#
# 1. Retire deploy du groupe docker (il n'a plus aucun rôle sous Docker).
# 2. Restreint sa clé SSH à un tunnel vers l'API Kubernetes (127.0.0.1:6443).
# 3. Crée les ServiceAccounts + droits RBAC (platform/github-deployer.yaml).
# 4. Vérifie les droits avec "kubectl auth can-i".
# 5. Génère un kubeconfig par environnement dans le dossier de marie (lisible par elle seule).

set -euo pipefail

PUBKEY="${1:?usage : sudo bash tools/08b-acces-github.sh <clé-publique.pub>}"
HERE="$(cd "$(dirname "$0")/.." && pwd)"      # ~/k8s
OUT_DIR="$(getent passwd "${SUDO_USER:-root}" | cut -d: -f6)"
OUT_USER="${SUDO_USER:-root}"

step() { printf '\n\033[1;34m==> %s\033[0m\n' "$1"; }
ok()   { printf '    \033[32mOK\033[0m %s\n' "$1"; }
bad()  { printf '    \033[31mPROBLÈME\033[0m %s\n' "$1"; FAILED=1; }
die()  { printf '    \033[31mERREUR\033[0m %s\n' "$1" >&2; exit 1; }
FAILED=0

[ "$(id -u)" -eq 0 ] || die "lancer avec sudo"
[ -f "$PUBKEY" ] || die "clé publique introuvable : $PUBKEY"
grep -q '^ssh-ed25519 ' "$PUBKEY" || die "$PUBKEY n'est pas une clé publique ed25519"

# ── 1. deploy n'a plus besoin de Docker ───────────────────────────────────────
step "1. Retrait de deploy du groupe docker"
if id -nG deploy | grep -qw docker; then
  gpasswd -d deploy docker >/dev/null
  ok "deploy retiré du groupe docker (fin de l'équivalent root)"
else
  ok "deploy n'était déjà plus dans le groupe docker"
fi

# ── 2. Clé SSH : tunnel vers l'API Kubernetes, et rien d'autre ────────────────
step "2. Clé SSH restreinte au tunnel"
# restrict             : coupe TOUT (redirections, agent, X11, terminal…)
# port-forwarding      : réautorise uniquement les redirections de port…
# permitopen=…         : …et seulement vers 127.0.0.1:6443 (l'API Kubernetes)
# command=…            : toute commande demandée est remplacée par ce message
OPTIONS='restrict,port-forwarding,permitopen="127.0.0.1:6443",command="echo Cette cle ne sert qu au tunnel Kubernetes"'
install -d -m 700 -o deploy -g deploy /home/deploy/.ssh
printf '%s %s\n' "$OPTIONS" "$(cat "$PUBKEY")" > /home/deploy/.ssh/authorized_keys
chown deploy:deploy /home/deploy/.ssh/authorized_keys
chmod 600 /home/deploy/.ssh/authorized_keys
# La crontab de deploy (étape 6) ne servait qu'à Compose.
crontab -u deploy -r 2>/dev/null || true
ok "authorized_keys remplacé (l'ancienne clé de déploiement ne fonctionne plus)"

# ── 3. Identités et droits dans le cluster ────────────────────────────────────
step "3. ServiceAccounts et RBAC"
kubectl apply -f "$HERE/platform/github-deployer.yaml"
for ns in jewel-staging jewel-prod; do
  # Kubernetes remplit le Secret du jeton quelques instants après sa création.
  until [ -n "$(kubectl -n "$ns" get secret github-deployer-token -o jsonpath='{.data.token}' 2>/dev/null)" ]; do sleep 1; done
done
ok "jetons générés"

# ── 4. Vérification des droits ────────────────────────────────────────────────
step "4. Vérification des droits (kubectl auth can-i)"
check() {   # check <attendu yes|no> <namespace du compte> <verbe> <ressource> [namespace cible]
  local expected=$1 sa_ns=$2 verb=$3 res=$4 target=${5:-$2} got sub=()
  # "pods/exec" = ressource "pods", sous-ressource "exec" : can-i attend --subresource.
  case "$res" in */*) sub=(--subresource="${res#*/}") ;; esac
  got=$(kubectl auth can-i "$verb" "${res%%/*}" "${sub[@]}" -n "$target" \
          --as="system:serviceaccount:$sa_ns:github-deployer" 2>/dev/null || true)
  if [ "$got" = "$expected" ]; then
    ok "$(printf '%-14s %-6s %-28s dans %-14s → %s' "$sa_ns" "$verb" "$res" "$target" "$got")"
  else
    bad "$(printf '%-14s %-6s %-28s dans %-14s → %s (attendu : %s)' "$sa_ns" "$verb" "$res" "$target" "$got" "$expected")"
  fi
}
check yes jewel-staging create deployments
check yes jewel-staging delete jobs
check yes jewel-staging create secrets
check no  jewel-staging create deployments jewel-prod        # staging ne touche pas la prod
check no  jewel-staging get    secrets     jewel-prod
check yes jewel-prod    patch  deployments
check no  jewel-prod    delete persistentvolumeclaims         # les données sont protégées
check no  jewel-prod    delete statefulsets
check no  jewel-prod    create pods/exec                      # pas d'accès dans les pods
check no  jewel-prod    create rolebindings                   # ne peut pas s'octroyer de droits
check no  jewel-prod    create namespaces
check no  jewel-prod    list   nodes
[ "$FAILED" -eq 0 ] || die "des droits ne sont pas ceux attendus : ne pas continuer"

# ── 5. Un kubeconfig par environnement ────────────────────────────────────────
step "5. Fichiers kubeconfig pour GitHub"
for ns in jewel-staging jewel-prod; do
  env_name=${ns#jewel-}                          # staging / prod
  file="$OUT_DIR/kubeconfig-$env_name.yaml"
  ca=$(kubectl -n "$ns" get secret github-deployer-token -o jsonpath='{.data.ca\.crt}')
  token=$(kubectl -n "$ns" get secret github-deployer-token -o jsonpath='{.data.token}' | base64 -d)
  ( umask 077
    cat > "$file" <<EOF
# Accès de GitHub Actions au namespace $ns (droits : platform/github-deployer.yaml).
# server = 127.0.0.1:6443 : l'extrémité locale du tunnel SSH ouvert par le pipeline.
apiVersion: v1
kind: Config
clusters:
  - name: mycharmly
    cluster:
      server: https://127.0.0.1:6443
      certificate-authority-data: $ca
users:
  - name: github-deployer
    user:
      token: $token
contexts:
  - name: $ns
    context:
      cluster: mycharmly
      user: github-deployer
      namespace: $ns
current-context: $ns
EOF
  )
  chown "$OUT_USER:$OUT_USER" "$file"
  # Test réel avec ce fichier (sur le serveur, 127.0.0.1:6443 EST l'API).
  if kubectl --kubeconfig "$file" get deployments >/dev/null 2>&1 \
     && ! kubectl --kubeconfig "$file" get nodes >/dev/null 2>&1; then
    ok "$file (fonctionne, et limité au namespace $ns)"
  else
    bad "$file ne se comporte pas comme prévu"
  fi
done
unset token

step "Terminé"
echo "    Copie chaque kubeconfig dans les secrets GitHub, puis supprime-les du serveur :"
echo "    rm ~/kubeconfig-staging.yaml ~/kubeconfig-prod.yaml"
