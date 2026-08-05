# Fragmento executado DENTRO da instância EC2 via AWS-RunShellScript.
#
# Não tem shebang de propósito: o workflow concatena um cabeçalho de variáveis
# antes deste arquivo e envia o resultado como um único script. As variáveis
# esperadas do cabeçalho são:
#
#   AWS_REGION, API_ENV_PARAM, IMAGE_URI, ECR_REGISTRY,
#   CONTAINER, HOST_PORT, CANARY_PORT,
#   CANARY_TIMEOUT, PROMOTE_TIMEOUT, STOP_GRACE
#
# Nenhum segredo chega por aqui: o script recebe apenas o NOME do parâmetro e
# resolve o conteúdo com a instance role, já dentro da instância.

set -eu

log() { echo "[deploy] $*"; }

ENV_DIR=/etc/empreganet
STATE_DIR=/var/lib/empreganet
ENV_FILE="$ENV_DIR/api.env"
PREV_FILE="$STATE_DIR/previous-image"
CANARY="${CONTAINER}-canary"

install -d -m 700 "$ENV_DIR" "$STATE_DIR"
umask 077

# --- Configuração ------------------------------------------------------------
log "Lendo configuração de $API_ENV_PARAM"
aws ssm get-parameter \
  --region "$AWS_REGION" \
  --name "$API_ENV_PARAM" \
  --with-decryption \
  --query Parameter.Value \
  --output text > "$ENV_FILE"
chmod 600 "$ENV_FILE"
log "Configuração gravada: $(wc -l < "$ENV_FILE") chaves"

log "Autenticando no ECR"
ECR_TOKEN="$(aws ecr get-login-password --region "$AWS_REGION")"
printf '%s' "$ECR_TOKEN" | docker login --username AWS --password-stdin "$ECR_REGISTRY" > /dev/null
unset ECR_TOKEN

log "Baixando $IMAGE_URI"
docker pull --quiet "$IMAGE_URI"

wait_ready() {
  name="$1"
  url="$2"
  timeout="$3"
  deadline=$(($(date +%s) + timeout))

  while [ "$(date +%s)" -lt "$deadline" ]; do
    if [ "$(docker inspect -f '{{.State.Running}}' "$name" 2>/dev/null || echo false)" != "true" ]; then
      CODE="$(docker inspect -f '{{.State.ExitCode}}' "$name" 2>/dev/null || echo '?')"
      log "Container $name parou de executar (exit $CODE)."
      return 1
    fi

    if curl -fsS --max-time 5 "$url" > /dev/null 2>&1; then
      return 0
    fi

    sleep 2
  done

  log "Tempo esgotado após ${timeout}s aguardando $url."
  log "O container continua em execução — pode ser uma migração mais longa que o teto."
  return 1
}

stop_container() {
  name="$1"
  if docker inspect "$name" > /dev/null 2>&1; then
    docker stop --timeout "$STOP_GRACE" "$name" > /dev/null 2>&1 || true
    docker rm -f "$name" > /dev/null 2>&1 || true
  fi
}

start_container() {
  image="$1"
  stop_container "$CONTAINER"
  docker run -d \
    --name "$CONTAINER" \
    --restart unless-stopped \
    --env-file "$ENV_FILE" \
    -p "${HOST_PORT}:8080" \
    --log-opt max-size=20m \
    --log-opt max-file=5 \
    "$image" > /dev/null
}

log "Subindo canário em 127.0.0.1:${CANARY_PORT} (teto de ${CANARY_TIMEOUT}s)"
stop_container "$CANARY"
docker run -d \
  --name "$CANARY" \
  --env-file "$ENV_FILE" \
  -p "127.0.0.1:${CANARY_PORT}:8080" \
  --log-opt max-size=20m \
  --log-opt max-file=2 \
  "$IMAGE_URI" > /dev/null

CANARY_OK=0
if wait_ready "$CANARY" "http://127.0.0.1:${CANARY_PORT}/health/ready" "$CANARY_TIMEOUT"; then
  CANARY_OK=1
fi

log "----- logs do canário (últimas 60 linhas) -----"
docker logs --tail 60 "$CANARY" 2>&1 || true
log "----- fim dos logs do canário -----"

stop_container "$CANARY"

if [ "$CANARY_OK" -ne 1 ]; then
  log "ERRO: o canário não ficou pronto."
  log "O container em produção NÃO foi alterado."
  exit 20
fi
log "Canário saudável. Schema e configuração validados contra as dependências reais."

PREVIOUS_IMAGE="$(docker inspect -f '{{.Config.Image}}' "$CONTAINER" 2>/dev/null || true)"
if [ -n "$PREVIOUS_IMAGE" ]; then
  printf '%s\n' "$PREVIOUS_IMAGE" > "$PREV_FILE"
  log "Versão anterior registrada para rollback: $PREVIOUS_IMAGE"
else
  log "Nenhum container anterior encontrado (primeiro deploy neste host)."
fi


log "Promovendo imagem nova na porta ${HOST_PORT} (teto de ${PROMOTE_TIMEOUT}s)"
start_container "$IMAGE_URI"

if wait_ready "$CONTAINER" "http://127.0.0.1:${HOST_PORT}/health/ready" "$PROMOTE_TIMEOUT"; then
  log "Deploy concluído e saudável."
  docker image prune -f > /dev/null 2>&1 || true
  exit 0
fi

# --- Rollback ----------------------------------------------------------------
log "ERRO: a imagem nova não ficou pronta na porta de produção."
log "----- logs do container (últimas 60 linhas) -----"
docker logs --tail 60 "$CONTAINER" 2>&1 || true
log "----- fim dos logs -----"

if [ ! -s "$PREV_FILE" ]; then
  log "CRÍTICO: não há versão anterior registrada. Serviço fora do ar."
  exit 23
fi

ROLLBACK_IMAGE="$(cat "$PREV_FILE")"
log "Revertendo para $ROLLBACK_IMAGE"
log "AVISO: o schema já foi migrado por este deploy e está à frente desta imagem."
start_container "$ROLLBACK_IMAGE"

if wait_ready "$CONTAINER" "http://127.0.0.1:${HOST_PORT}/health/ready" "$PROMOTE_TIMEOUT"; then
  log "Rollback concluído: serviço restaurado na versão anterior."
  exit 21
fi

log "CRÍTICO: o rollback também não ficou saudável. Serviço fora do ar."
# OBS: Caso ocorrer erro, a correção é seguir para frente com um fix, não para trás."
exit 22
