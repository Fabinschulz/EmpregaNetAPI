#!/usr/bin/env bash
# Falha se algum security group aceitar entrada de 0.0.0.0/0 ou ::/0 fora do módulo alb (a única porta da internet),
# ou se usar bloco ingress inline (as regras são recursos próprios, para a verificação enxergar todas).
# uso: bash infra/scripts/check-open-ingress.sh [diretório]   (padrão: infra)
set -euo pipefail

root="${1:-infra}"
failures=0

while IFS= read -r file; do
  case "$file" in
    */modules/alb/*) continue ;;
  esac

  result="$(awk '
    /^resource "(aws_vpc_security_group_ingress_rule|aws_security_group_rule)"/ { in_rule = 1 }
    in_rule && /0\.0\.0\.0\/0|::\/0/ { printf "%s:%d: entrada aberta para a internet\n", FILENAME, FNR }
    in_rule && /^}/ { in_rule = 0 }
    /^[[:space:]]+ingress[[:space:]]*\{/ { printf "%s:%d: bloco ingress inline; use aws_vpc_security_group_ingress_rule\n", FILENAME, FNR }
  ' "$file")"

  if [ -n "$result" ]; then
    echo "$result"
    failures=$((failures + 1))
  fi
done < <(find "$root" -name '*.tf' -not -path '*/.terraform/*' | sort)

if [ "$failures" -gt 0 ]; then
  echo "FALHA: entrada aberta fora do módulo alb." >&2
  exit 1
fi

echo "ok: nenhuma entrada aberta para a internet fora do módulo alb."
