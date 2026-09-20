'use client';

const LAST_SEEN_STATUS_KEY = 'empreganet_applications_last_seen_status';

type LastSeenStatusMap = Record<number, string>;

const isBrowser: () => boolean = () => typeof window !== 'undefined';

function readMap(): LastSeenStatusMap {
  if (!isBrowser()) return {};

  try {
    const raw = window.localStorage.getItem(LAST_SEEN_STATUS_KEY);
    if (!raw) return {};

    const parsed: unknown = JSON.parse(raw);
    if (typeof parsed !== 'object' || parsed === null) return {};
    return parsed as LastSeenStatusMap;
  } catch {
    return {};
  }
}

function writeMap(map: LastSeenStatusMap): void {
  if (!isBrowser()) return;

  try {
    window.localStorage.setItem(LAST_SEEN_STATUS_KEY, JSON.stringify(map));
  } catch {
    // Sem persistência o indicador de "atualizado" simplesmente não aparece; o status real
    // continua correto, só perde o destaque de novidade.
  }
}

/** Snapshot do último status visto por candidatura. Ler uma vez por visita à lista. */
export const readLastSeenApplicationStatuses: () => LastSeenStatusMap = () => readMap();

/** Compara contra um snapshot já lido: mudou de status (ou nunca foi visto) desde então? */
export function hasApplicationStatusChanged(
  snapshot: LastSeenStatusMap,
  applicationId: number,
  status: string
): boolean {
  const lastSeen = snapshot[applicationId];
  return lastSeen === undefined || lastSeen !== status;
}

/** Marca as candidaturas exibidas como vistas, com o status atual de cada uma. */
export function markApplicationStatusesAsSeen(applications: readonly { id: number; status: string }[]): void {
  if (applications.length === 0) return;

  const map = readMap();
  for (const application of applications) {
    map[application.id] = application.status;
  }
  writeMap(map);
}
