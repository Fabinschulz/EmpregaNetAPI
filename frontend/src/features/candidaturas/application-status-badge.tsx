'use client';

import { StatusBadge, type StatusTone } from '@/components';
import { applicationStatusLabels, parseApplicationStatus, type ApplicationStatus } from './service';

const STATUS_TONE: Record<ApplicationStatus, StatusTone> = {
  Pending: 'warning',
  Processing: 'warning',
  Approved: 'positive',
  Finished: 'positive',
  Rejected: 'negative',
  Canceled: 'negative',
  Timeout: 'negative',
  Error: 'negative'
};

/**
 * Badge do status, com rótulo pt-BR e cor semântica.
 * Valores desconhecidos (fora do enum) são exibidos como texto neutro.
 */
export function ApplicationStatusBadge({ status }: { status: string | null | undefined }) {
  const parsed = parseApplicationStatus(status);
  if (!parsed) return <StatusBadge label={status} tone="neutral" />;
  return <StatusBadge label={applicationStatusLabels[parsed]} tone={STATUS_TONE[parsed]} />;
}
