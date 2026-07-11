'use client';

import { Badge } from '@/components';
import { applicationStatusLabels, parseApplicationStatus, type ApplicationStatus } from '@/services';

const STATUS_BADGE_VARIANT: Record<ApplicationStatus, 'default' | 'secondary' | 'destructive'> = {
  Pending: 'secondary',
  Processing: 'secondary',
  Approved: 'default',
  Finished: 'default',
  Rejected: 'destructive',
  Canceled: 'destructive',
  Timeout: 'destructive',
  Error: 'destructive'
};

/**
 * Badge do status de uma candidatura, com rótulo pt-BR e cor semântica.
 * Valores desconhecidos (fora do enum) são exibidos como texto neutro.
 */
export function ApplicationStatusBadge({ status }: { status: string | null | undefined }) {
  const parsed = parseApplicationStatus(status);
  if (!parsed) return <Badge variant="secondary">{status ?? '—'}</Badge>;
  return <Badge variant={STATUS_BADGE_VARIANT[parsed]}>{applicationStatusLabels[parsed]}</Badge>;
}
