/** Exibido quando não há data (a API devolve string vazia nesse caso). */
const EMPTY = '-';

/**
 * Formato da datas: "dd/MM/yyyy HH:mm:ss" (a hora é opcional).
 * `BaseViewModel` do backend, que já converte para o fuso de Brasília e formata
 * em pt-BR antes de serializar.
 */
const API_DATE_TIME = /^(\d{2})\/(\d{2})\/(\d{4})(?:[ T](\d{2}):(\d{2})(?::\d{2})?)?$/;

const isoDateFormatter = new Intl.DateTimeFormat('pt-BR', { dateStyle: 'short' });
const isoTimeFormatter = new Intl.DateTimeFormat('pt-BR', { timeStyle: 'short' });

type DateTimeParts = { date: string; time?: string };


function parseApiDateTime(value?: string | null): DateTimeParts | null {
  const raw = value?.trim();
  if (!raw) return null;

  const match = API_DATE_TIME.exec(raw);
  if (match) {
    const [, day, month, year, hour, minute] = match;
    return {
      date: `${day}/${month}/${year}`,
      time: hour && minute ? `${hour}:${minute}` : undefined
    };
  }

  const parsed = new Date(raw);
  if (Number.isNaN(parsed.getTime())) return null;

  return { date: isoDateFormatter.format(parsed), time: isoTimeFormatter.format(parsed) };
}

/** Só a data (`18/07/2026`). Preferível em tabelas, onde a hora costuma ser ruído. */
export function formatDate(value?: string | null): string {
  return parseApiDateTime(value)?.date ?? EMPTY;
}

/** Data e hora sem os segundos (`18/07/2026 18:54`). Para telas de detalhe. */
export function formatDateTime(value?: string | null): string {
  const parts = parseApiDateTime(value);
  if (!parts) return EMPTY;
  return parts.time ? `${parts.date} ${parts.time}` : parts.date;
}
