export { cn } from '../lib/utils';

export function getFieldErrorMessage(path: string, errors: unknown): string | undefined {
  const v = getObjectPropertyValue(path, errors);
  if (v && typeof v === 'object' && v !== null && 'message' in v) {
    const m = (v as { message?: unknown }).message;
    return typeof m === 'string' ? m : undefined;
  }
  return undefined;
}

export function getObjectPropertyValue(path: string, obj: unknown): unknown {
  if (obj == null || typeof obj !== 'object') return undefined;
  const parts = path.split('.').filter(Boolean);
  let current: unknown = obj;
  for (const key of parts) {
    if (current == null || typeof current !== 'object') return undefined;
    current = (current as Record<string, unknown>)[key];
  }
  return current;
}

export function truncateText(text: string, maxLength: number): string {
  if (text.length <= maxLength) return text;
  return `${text.slice(0, maxLength)}…`;
}
