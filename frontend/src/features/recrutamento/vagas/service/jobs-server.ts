// Guard de fronteira: falha o build imediatamente (apontando para o import culpado) se este
// módulo for arrastado para o bundle do cliente; ex.: alguém reexportá-lo no barrel
// `service/index.ts`, que é consumido por Client Components.
import 'server-only';

import { getPublicEnv } from '@/utils/lib/env';
import { cacheLife, cacheTag } from 'next/cache';
import { jobSchema, type JobDto } from './jobs-schema';

/**
 * Base URL usada pelo processo Next (server-side) para alcançar a API. Em produção o
 * servidor pode precisar de um host interno diferente do `NEXT_PUBLIC_*` (a URL vista pelo
 * browser) — ex.: rede de containers/k8s. Cai de volta para a pública quando não definida.
 */
function serverApiBaseUrl(): string {
  return process.env.API_INTERNAL_BASE_URL?.trim() || getPublicEnv().NEXT_PUBLIC_API_BASE_URL;
}

/**
 * Busca uma vaga no servidor (Server Components / `generateMetadata`) sem passar pelo
 * axios do cliente: o endpoint `/api/jobs/:id` é público, então não há cookie de sessão
 * envolvido. Reutiliza o mesmo `jobSchema` da camada de serviço como validação de fronteira,
 * mantendo o contrato único com a API .NET.
 *
 * `'use cache'` + `cacheLife('minutes')` dão comportamento tipo ISR (Incremental Static Regeneration): a resposta é memoizada
 * por `id` e revalidada por tempo. `cacheTag(`job:${id}`)` prepara invalidação dirigida via
 * `revalidateTag(`job:${id}`)`, ainda SEM chamador (mutações de vaga são client -> .NET),
 * então hoje a atualização acontece apenas por expiração do `cacheLife`.
 *
 * Distingue os casos: `404 -> null` (vira `notFound()`); resposta inválida ao `jobSchema`
 * LANÇA (como o resto da camada de serviço), para não mascarar drift de contrato como 404.
 *
 * Uso exclusivamente server-side, importa `next/cache`, indisponível no cliente.
 */
export async function getJobCached(id: number): Promise<JobDto | null> {
  'use cache';
  cacheLife('minutes');
  cacheTag(`job:${id}`);

  if (!Number.isFinite(id) || id <= 0) return null;

  const res = await fetch(`${serverApiBaseUrl()}/api/jobs/${id}`, {
    headers: { Accept: 'application/json' }
  });

  if (res.status === 404) return null;
  if (!res.ok) throw new Error(`Falha ao carregar a vaga ${id} (HTTP ${res.status}).`);

  const data: unknown = await res.json();
  return jobSchema.parse(data);
}
