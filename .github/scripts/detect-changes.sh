#!/usr/bin/env bash
# Uso: detect-changes.sh <event_name> <base_sha> <head_sha>
# Imprime backend=true|false e frontend=true|false, uma linha cada, para anexar ao $GITHUB_OUTPUT.
# Workflows reutilizáveis não aceitam filtro de paths, por isso a decisão fica neste script.
set -euo pipefail

event="${1:-}"
base="${2:-}"
head="${3:-}"

backend_pattern='^(backend/|\.github/workflows/(main-deploy|ci-backend|cd-image|cd-ec2)\.yml$|\.github/(actions|scripts)/)'
frontend_pattern='^(frontend/|\.github/workflows/(main-deploy|ci-frontend)\.yml$)'

# Sem uma base conhecida não há como provar que nada mudou; pular o CD em silêncio seria pior que rodá-lo à toa.
everything_changed() {
  echo "$1 Todas as áreas tratadas como alteradas." >&2
  echo "backend=true"
  echo "frontend=true"
  exit 0
}

if [ "$event" = "workflow_dispatch" ]; then
  everything_changed "Disparo manual."
fi
if [[ "$base" =~ ^0+$ ]]; then
  everything_changed "Push sem commit anterior (${base})."
fi
if ! git cat-file -e "${base}^{commit}" 2>/dev/null; then
  everything_changed "Commit base '${base}' não encontrado no histórico."
fi

# Três pontos: compara com o ancestral comum, então num pull request só entra o que a branch mudou.
changed="$(git diff --name-only "${base}...${head}")"

if grep -Eq "$backend_pattern" <<<"$changed"; then backend=true; else backend=false; fi
if grep -Eq "$frontend_pattern" <<<"$changed"; then frontend=true; else frontend=false; fi

echo "backend=${backend}"
echo "frontend=${frontend}"
