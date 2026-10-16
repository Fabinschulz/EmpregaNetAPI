import { jobStatusLabel, type JobStatus } from '@/features/recrutamento/vagas/domain';
import { formatDate } from '@/shared/utils';

const API_DATE = /^(\d{2})\/(\d{2})\/(\d{4})/;
const DAY_MS = 24 * 60 * 60 * 1000;

const longDateFormatter = new Intl.DateTimeFormat('pt-BR', {
  day: 'numeric',
  month: 'long',
  year: 'numeric',
  timeZone: 'UTC'
});

/**
 * Dia civil (UTC, sem hora) de uma data da API. Aceita ISO e o formato "dd/MM/yyyy" do
 * `BaseViewModel`. Devolve `null` para vazio ou inválido, para o recuo gracioso da tela.
 */
function toCalendarDay(value?: string | null): number | null {
  const raw = value?.trim();
  if (!raw) return null;

  const br = API_DATE.exec(raw);
  if (br) return Date.UTC(Number(br[3]), Number(br[2]) - 1, Number(br[1]));

  const parsed = new Date(raw);
  if (Number.isNaN(parsed.getTime())) return null;
  return Date.UTC(parsed.getFullYear(), parsed.getMonth(), parsed.getDate());
}

export function closureReason(status: JobStatus): string {
  switch (status) {
    case 'ClosedByFulfillment':
      return 'Todas as posições foram preenchidas.';
    case 'ClosedManually':
    case 'Active':
      return 'A empresa encerrou as inscrições para esta vaga.';
    default: {
      const unreachable: never = status;
      return unreachable;
    }
  }
}

export function formatLongDate(value?: string | null): string | null {
  const day = toCalendarDay(value);
  return day === null ? null : longDateFormatter.format(new Date(day));
}

export function describeOpenPeriod(publishedAt?: string | null, closedAt?: string | null): string | null {
  const start = toCalendarDay(publishedAt);
  const end = toCalendarDay(closedAt);
  if (start === null || end === null || end < start) return null;

  const days = Math.round((end - start) / DAY_MS);
  if (days === 0) return 'Ficou aberta por menos de um dia';
  return `Ficou aberta por ${days} ${days === 1 ? 'dia' : 'dias'}`;
}

export function closureStatusLabel(status: JobStatus, positions?: number | null): string {
  return status === 'ClosedByFulfillment' ? jobStatusLabel(status, 'candidate', positions) : 'Encerrada';
}

export function closureTitle(status: JobStatus): string {
  return status === 'ClosedByFulfillment' ? 'Boas notícias: esta vaga foi preenchida' : 'Processo seletivo encerrado';
}

type ClosureJob = {
  status: JobStatus;
  positions?: number | null;
  publishedAt?: string | null;
  closedAt?: string | null;
};

export type JobClosureSummary = {
  statusLabel: string;
  title: string;
  headline: string;
  reason: string;
  publishedOn: string | null;
  closedOn: string | null;
  openPeriod: string | null;
};

export function summarizeJobClosure(job: ClosureJob): JobClosureSummary {
  const closedLong = formatLongDate(job.closedAt);
  const publishedLong = formatLongDate(job.publishedAt);

  return {
    statusLabel: closureStatusLabel(job.status, job.positions),
    title: closureTitle(job.status),
    headline: closedLong ? `Esta vaga foi encerrada em ${closedLong}.` : 'Esta vaga foi encerrada.',
    reason: closureReason(job.status),
    publishedOn: publishedLong ? formatDate(job.publishedAt) : null,
    closedOn: closedLong ? formatDate(job.closedAt) : null,
    openPeriod: describeOpenPeriod(job.publishedAt, job.closedAt)
  };
}
