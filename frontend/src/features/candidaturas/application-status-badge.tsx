'use client';

import { StatusBadge, type StatusTone } from '@/shared/components';
import {
    applicationStatusLabel,
    parseApplicationStatus,
    type ApplicationStatus,
    type ApplicationStatusAudience
} from './domain';

const STATUS_TONE: Record<ApplicationStatus, StatusTone> = {
  Pending: 'warning',
  Processing: 'warning',
  Approved: 'positive',
  Finished: 'positive',
  Rejected: 'negative',
  Canceled: 'negative',
  Timeout: 'negative',
  Error: 'negative',
  CanceledByCandidate: 'neutral'
};

type ApplicationStatusBadgeProps = {
  status: string | null | undefined;
  count?: number;
  audience?: ApplicationStatusAudience;
};

/**
 * Badge do status, com rótulo pt-BR e cor semântica.
 * Valores desconhecidos (fora do enum) são exibidos como texto neutro.
 */
export function ApplicationStatusBadge({ status, count, audience = 'recruiter' }: ApplicationStatusBadgeProps) {
  const parsed = parseApplicationStatus(status);
  const label = parsed ? applicationStatusLabel(parsed, audience) : status;
  const withCount = label && count !== undefined ? `${label} · ${count}` : label;

  if (!parsed) return <StatusBadge label={withCount} tone="neutral" />;
  return <StatusBadge label={withCount} tone={STATUS_TONE[parsed]} />;
}
