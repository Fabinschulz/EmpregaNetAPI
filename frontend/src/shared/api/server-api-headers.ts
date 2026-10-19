import 'server-only';

/**
 * Cabeçalhos das chamadas do SSR à API. O IP das tarefas muda a cada início, então ele se identifica por
 * `INTERNAL_API_SECRET`; sem o segredo nada é enviado e a API o trata como um cliente comum.
 */
export function serverApiHeaders(): Record<string, string> {
  const headers: Record<string, string> = { Accept: 'application/json' };

  const secret = process.env.INTERNAL_API_SECRET?.trim();
  if (secret) headers['X-Internal-Caller'] = secret;

  return headers;
}
