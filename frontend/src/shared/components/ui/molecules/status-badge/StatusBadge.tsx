'use client';

import { Badge } from '../../atoms/badge';

/** Tom semântico de um status; mapeia para a cor do badge. */
export type StatusTone = 'positive' | 'negative' | 'warning' | 'neutral';

const TONE_TO_VARIANT: Record<StatusTone, 'success' | 'destructive' | 'warning' | 'secondary'> = {
  positive: 'success',
  negative: 'destructive',
  warning: 'warning',
  neutral: 'secondary'
};

// Termos (pt-BR e enum do backend, minúsculos) agrupados por tom. Genérico: serve para
// qualquer coluna de situação/status de qualquer tabela do sistema.
const TONE_KEYWORDS: Record<Exclude<StatusTone, 'neutral'>, readonly string[]> = {
  positive: [
    'ativo',
    'ativa',
    'aprovada',
    'aprovado',
    'concluída',
    'concluido',
    'concluída',
    'finished',
    'approved',
    'active'
  ],
  negative: [
    'excluído',
    'excluido',
    'excluída',
    'inativo',
    'inativa',
    'encerrada',
    'encerrado',
    'reprovada',
    'reprovado',
    'cancelada',
    'cancelado',
    'rejeitada',
    'rejected',
    'canceled',
    'error',
    'erro',
    'timeout',
    'expirado'
  ],
  warning: ['pendente', 'em análise', 'em analise', 'processando', 'pending', 'processing']
};

/** Infere o tom a partir do texto do status quando o chamador não informa explicitamente. */
export function inferStatusTone(value: string | null | undefined): StatusTone {
  if (!value) return 'neutral';
  const normalized = value.trim().toLowerCase();
  for (const tone of ['positive', 'negative', 'warning'] as const) {
    if (TONE_KEYWORDS[tone].some((keyword) => normalized === keyword)) return tone;
  }
  return 'neutral';
}

export type StatusBadgeProps = {
  /** Texto exibido (já traduzido/formatado). */
  label: string | null | undefined;
  /** Tom explícito; se omitido, é inferido a partir de `label`. */
  tone?: StatusTone;
  /** Texto exibido quando `label` é vazio. Padrão: "—". */
  emptyText?: string;
};

/**
 * Badge de status com cor semântica (verde=positivo, vermelho=negativo, âmbar=atenção,
 * cinza=neutro). Genérico para qualquer coluna de situação/status; o tom pode ser
 * informado ou inferido do próprio texto.
 */
export function StatusBadge({ label, tone, emptyText = '—' }: StatusBadgeProps) {
  const text = label?.trim();
  if (!text) return <Badge variant="secondary">{emptyText}</Badge>;
  const resolvedTone = tone ?? inferStatusTone(text);
  return <Badge variant={TONE_TO_VARIANT[resolvedTone]}>{text}</Badge>;
}
