// Health check do contêiner (ECS/ALB): responde 200 sem tocar na API nem no banco. Se a API cair, as tarefas do
// frontend não devem ser recicladas por isso — a falha dela é da API, não do processo Next.

export function GET(): Response {
  return new Response('ok', { status: 200, headers: { 'Cache-Control': 'no-store' } });
}
