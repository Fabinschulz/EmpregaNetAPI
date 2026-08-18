'use client';

const SESSION_METADATA_KEY = 'empreganet_session_meta';
const SESSION_METADATA_EVENT = 'empreganet:session-meta';

export type SessionMetadata = {
  roles: string[];
  username: string | null;
  email: string | null;
};

function isBrowser(): boolean {
  return typeof window !== 'undefined';
}

function readRaw(): string | null {
  try {
    return window.localStorage.getItem(SESSION_METADATA_KEY);
  } catch {
    return null;
  }
}

function parseMetadata(raw: string): SessionMetadata | null {
  try {
    const parsed = JSON.parse(raw) as SessionMetadata;
    if (!Array.isArray(parsed.roles)) return null;
    return parsed;
  } catch {
    return null;
  }
}

// Cache referencial para o snapshot do useSyncExternalStore (mesmo raw -> mesmo objeto).
let metadataCacheRaw: string | null = null;
let metadataCacheParsed: SessionMetadata | null = null;

/** Snapshot dos metadados de sessão (estável referencialmente para useSyncExternalStore). */
export function getSessionMetadataSnapshot(): SessionMetadata | null {
  if (!isBrowser()) return null;

  const raw = readRaw();
  if (raw !== metadataCacheRaw) {
    metadataCacheRaw = raw;
    metadataCacheParsed = raw ? parseMetadata(raw) : null;
  }
  return metadataCacheParsed;
}

/**
 * Subscreve mudanças dos metadados: evento custom (mesma aba) + `storage` (outras abas),
 * o que sincroniza login/logout entre abas automaticamente.
 */
export function subscribeSessionMetadata(callback: () => void): () => void {
  if (!isBrowser()) return () => undefined;

  window.addEventListener(SESSION_METADATA_EVENT, callback);
  window.addEventListener('storage', callback);
  return () => {
    window.removeEventListener(SESSION_METADATA_EVENT, callback);
    window.removeEventListener('storage', callback);
  };
}

function notifySessionMetadataChanged(): void {
  window.dispatchEvent(new Event(SESSION_METADATA_EVENT));
}

export function saveSessionMetadata(meta: SessionMetadata): void {
  if (!isBrowser()) return;

  try {
    window.localStorage.setItem(SESSION_METADATA_KEY, JSON.stringify(meta));
  } catch {
    // Sem persistência a sessão continua válida: a credencial é o cookie httpOnly. A UI só
    // perde o nome exibido depois de um reload.
  }

  notifySessionMetadataChanged();
}

export function clearSessionMetadata(): void {
  if (!isBrowser()) return;

  try {
    window.localStorage.removeItem(SESSION_METADATA_KEY);
  } catch {
    // Ignorado: o logout real é o cookie limpo pelo servidor.
  }

  notifySessionMetadataChanged();
}
