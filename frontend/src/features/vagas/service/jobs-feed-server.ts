import 'server-only';

import { getPublicEnv } from '@/shared/utils';
import { cacheLife } from 'next/cache';
import type { JobsFeedQueryParams } from './jobs-feed-params';
import {
    jobVocabularyResponseSchema,
    jobsFeedResponseSchema,
    type JobVocabularyResponse,
    type JobsFeedResponse
} from './jobs-feed-response-schema';

function serverApiBaseUrl(): string {
  return process.env.API_INTERNAL_BASE_URL?.trim() || getPublicEnv().NEXT_PUBLIC_API_BASE_URL;
}

function buildQueryString(params: JobsFeedQueryParams): string {
  const search = new URLSearchParams();

  Object.entries(params).forEach(([key, value]) => {
    if (value === undefined || value === null) return;

    if (Array.isArray(value)) {
      value.forEach((item) => search.append(key, String(item)));
      return;
    }

    search.append(key, String(value));
  });

  return search.toString();
}

/**
 * Busca a primeira página do feed no servidor, para o HTML inicial de `/vagas` já sair com vagas
 * (SEO e primeiro paint). O endpoint é público, então não há cookie de sessão envolvido.
 *
 * Reutiliza o `jobsFeedResponseSchema` da camada de serviço como validação de fronteira,
 * mantendo contrato único com a API .NET.
 *
 * `'use cache'` + `cacheLife('minutes')` memoizam por combinação de filtros e revalidam por
 * tempo. Não há `cacheTag` por vaga aqui de propósito: uma entrada do feed depende de muitas
 * vagas, e invalidar todas as combinações a cada mutação anularia o cache. A janela curta é a
 * troca consciente - o cartão pode ficar minutos desatualizado, a página de detalhe não.
 *
 * Falha de rede ou resposta inválida **lança**: o Server Component decide o que mostrar.
 */
export async function getJobsFeedCached(params: JobsFeedQueryParams): Promise<JobsFeedResponse> {
  'use cache';
  cacheLife('minutes');

  const query = buildQueryString(params);
  const res = await fetch(`${serverApiBaseUrl()}/api/jobs/feed?${query}`, {
    headers: { Accept: 'application/json' }
  });

  if (!res.ok) {
    throw new Error(`Falha ao carregar o feed de vagas (HTTP ${res.status}).`);
  }

  const data: unknown = await res.json();
  return jobsFeedResponseSchema.parse(data);
}

/**
 * Vocabulário de requisitos e benefícios, resolvido no servidor.
 *
 * Buscá-lo aqui e passá-lo por prop evita a cascata que existiria se o painel de filtros o
 * pedisse ao montar: o painel é a primeira coisa que o utilizador vê. Como só muda com deploy,
 * a janela de cache é longa.
 */
export async function getJobVocabularyCached(): Promise<JobVocabularyResponse> {
  'use cache';
  cacheLife('days');

  const res = await fetch(`${serverApiBaseUrl()}/api/jobs/vocabulary`, {
    headers: { Accept: 'application/json' }
  });

  if (!res.ok) {
    throw new Error(`Falha ao carregar o vocabulário de vagas (HTTP ${res.status}).`);
  }

  const data: unknown = await res.json();
  return jobVocabularyResponseSchema.parse(data);
}
