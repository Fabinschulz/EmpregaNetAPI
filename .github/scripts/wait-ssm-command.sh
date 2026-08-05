#!/usr/bin/env bash
#
# Aguarda a conclusão de um comando do SSM, imprime a saída da instância e
# propaga o resultado para o job.
#
# Substitui o `sleep 10` seguido de um `get-command-invocation` cujo retorno era
# apenas impresso: naquele formato, um container em crash loop produzia um job
# verde. Aqui, qualquer status diferente de Success derruba o step.
#
# O waiter nativo (`aws ssm wait command-executed`) não serve: desiste em ~100 s
# (20 tentativas x 5 s), curto demais para um deploy com canário.

set -uo pipefail

CMD_ID="${1:?uso: wait-ssm-command.sh <command-id> <instance-id> <region> [timeout]}"
INSTANCE_ID="${2:?instance-id obrigatório}"
REGION="${3:?region obrigatória}"
TIMEOUT="${4:-1200}"

echo "Aguardando comando $CMD_ID em $INSTANCE_ID (timeout ${TIMEOUT}s)..."

STATUS=Pending
DEADLINE=$((SECONDS + TIMEOUT))

while [ "$SECONDS" -lt "$DEADLINE" ]; do

  STATUS="$(aws ssm get-command-invocation \
    --region "$REGION" \
    --command-id "$CMD_ID" \
    --instance-id "$INSTANCE_ID" \
    --query Status --output text 2>/dev/null || echo Pending)"

  case "$STATUS" in
    Success | Failed | Cancelled | TimedOut) break ;;
  esac

  sleep 5
done

INVOCATION="$(aws ssm get-command-invocation \
  --region "$REGION" \
  --command-id "$CMD_ID" \
  --instance-id "$INSTANCE_ID" \
  --output json 2>/dev/null || echo '{}')"


echo "::group::Saída da instância"
echo "$INVOCATION" | jq -r '.StandardOutputContent // "(vazio)"'
echo "::endgroup::"

ERR="$(echo "$INVOCATION" | jq -r '.StandardErrorContent // ""')"
if [ -n "$ERR" ]; then
  echo "::group::Erro padrão da instância"
  echo "$ERR"
  echo "::endgroup::"
fi

CODE="$(echo "$INVOCATION" | jq -r '.ResponseCode // "-1"')"
echo "Status final: $STATUS (exit code $CODE)"

if [ "$STATUS" != "Success" ]; then
  case "$CODE" in
    20) HINT="canário não subiu; o container em produção não foi tocado" ;;
    21) HINT="imagem nova falhou e o rollback para a anterior teve êxito" ;;
    22) HINT="CRÍTICO: rollback também falhou; serviço provavelmente fora do ar" ;;
    23) HINT="CRÍTICO: imagem nova falhou e não havia versão anterior registrada" ;;
    30) HINT="digest do script de migrações divergiu do gerado no CI" ;;
    *) HINT="ver a saída da instância acima" ;;
  esac
  echo "::error title=Comando SSM falhou::status=$STATUS exit=$CODE - $HINT (command-id=$CMD_ID)"
  exit 1
fi

echo "Comando concluído com sucesso."
