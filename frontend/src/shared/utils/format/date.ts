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

const MINUTE = 60_000;
const HOUR = 60 * MINUTE;
const DAY = 24 * HOUR;

/**
 * Tempo decorrido em linguagem natural ("há 3 dias"), como no cartão do feed.
 *
 * `now` é **parâmetro**, não `Date.now()` interno: a função é chamada durante a renderização e
 * ler o relógio aqui congelaria o valor no build (`cacheComponents`) além de tornar o resultado
 * impossível de testar. Quem chama decide qual instante usar - ver `useRelativeTime`.
 *
 * Acima de 30 dias devolve a data absoluta: "há 87 dias" não ajuda ninguém a situar-se.
 */
export function formatRelativeTime(value: string | null | undefined, now: number): string {
  const raw = value?.trim();
  if (!raw) return EMPTY;

  const timestamp = new Date(raw).getTime();
  if (Number.isNaN(timestamp)) return EMPTY;

  const elapsed = now - timestamp;

  // Relógio do cliente adiantado em relação ao servidor é comum; tratar como "agora" evita
  // exibir "há -2 minutos".
  if (elapsed < MINUTE) return 'agora mesmo';

  if (elapsed < HOUR) {
    const minutes = Math.floor(elapsed / MINUTE);
    return `há ${minutes} ${minutes === 1 ? 'minuto' : 'minutos'}`;
  }

  if (elapsed < DAY) {
    const hours = Math.floor(elapsed / HOUR);
    return `há ${hours} ${hours === 1 ? 'hora' : 'horas'}`;
  }

  const days = Math.floor(elapsed / DAY);
  if (days === 1) return 'ontem';
  if (days <= 30) return `há ${days} dias`;

  return formatDate(raw);
}
