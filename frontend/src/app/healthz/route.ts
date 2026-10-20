// Health check do contêiner: não toca na API, para uma falha dela não reciclar as tarefas do frontend.

export function GET(): Response {
  return new Response('ok', { status: 200, headers: { 'Cache-Control': 'no-store' } });
}
