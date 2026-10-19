#!/usr/bin/env bash
# Valida a imagem do frontend sem depender da API: constrói, confere que roda sem root, que nenhuma URL de
# desenvolvimento foi embutida e que o health check responde (inclusive sem cookie, sem cair no guard de login).
# uso: bash scripts/verify-container.sh [tag]   (rode de dentro de frontend/)
set -euo pipefail

TAG="${1:-empreganet-web:verify}"
NAME="empreganet-web-verify-$$"
PORT="${VERIFY_PORT:-3199}"

cleanup() { docker rm -f "$NAME" >/dev/null 2>&1 || true; }
trap cleanup EXIT

echo "==> build"
docker build -t "$TAG" .

echo "==> usuário do processo (não pode ser root)"
USER_ID="$(docker run --rm --entrypoint id "$TAG" -u)"
[ "$USER_ID" != "0" ] || { echo "FALHA: a imagem roda como root"; exit 1; }
echo "uid=$USER_ID"

echo "==> URLs de desenvolvimento embutidas"
LEAKS="$(docker run --rm --entrypoint sh "$TAG" -c \
  'grep -rIl -e "localhost:5225" -e "localhost:5143" -e "127.0.0.1:5225" /app 2>/dev/null || true')"
[ -z "$LEAKS" ] || { echo "FALHA: URL de desenvolvimento encontrada em:"; echo "$LEAKS"; exit 1; }
echo "nenhuma"

echo "==> subida e /healthz"
docker run -d --name "$NAME" -p "$PORT:3000" "$TAG" >/dev/null
for _ in $(seq 1 30); do
  [ "$(curl -s -o /dev/null -w '%{http_code}' "http://localhost:$PORT/healthz" || true)" = "200" ] && { echo "healthz 200"; exit 0; }
  sleep 1
done

echo "FALHA: /healthz não respondeu 200 em 30 s"
docker logs "$NAME" 2>&1 | tail -20
exit 1
