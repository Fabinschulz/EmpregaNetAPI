import 'server-only';

/**
 * Cabeçalhos de toda chamada do servidor Next.js (SSR) à API.
 *
 * O SSR chama a API a partir de tarefas cujo IP muda a cada início, e todo o tráfego do site passa por poucas
 * tarefas. Por isso a API não o reconhece por IP: ele se identifica com um segredo compartilhado
 * (`INTERNAL_API_SECRET`) e recebe um balde de requisições próprio, mais largo, porém limitado.
 *
 * Sem o segredo configurado nada é enviado, e a API trata o SSR como um cliente comum.
 */
export function serverApiHeaders(): Record<string, string> {
  const headers: Record<string, string> = { Accept: 'application/json' };

  const secret = process.env.INTERNAL_API_SECRET?.trim();
  if (secret) headers['X-Internal-Caller'] = secret;

  return headers;
}
