import { axiosApi, createAxiosConfig } from '@/shared/api';
import type { JobsFeedQueryParams } from './jobs-feed-params';
import {
  jobFeedInteractionsResponseSchema,
  jobVocabularyResponseSchema,
  jobsFeedResponseSchema,
  type JobFeedInteractionsResponse,
  type JobVocabularyResponse,
  type JobsFeedResponse
} from './jobs-feed-response-schema';

export const MAX_INTERACTION_IDS = 100;

function chunk<T>(items: T[], size: number): T[][] {
  const chunks: T[][] = [];
  for (let index = 0; index < items.length; index += size) {
    chunks.push(items.slice(index, index + size));
  }
  return chunks;
}

export async function fetchJobsFeed(params: JobsFeedQueryParams): Promise<JobsFeedResponse> {
  const res = await axiosApi.get<unknown>('/api/jobs/feed', { params });
  return jobsFeedResponseSchema.parse(res.data);
}

export async function fetchJobVocabulary(): Promise<JobVocabularyResponse> {
  const res = await axiosApi.get<unknown>('/api/jobs/vocabulary');
  return jobVocabularyResponseSchema.parse(res.data);
}

export async function fetchJobFeedInteractions(jobIds: number[]): Promise<JobFeedInteractionsResponse> {
  if (jobIds.length === 0) {
    return { appliedJobIds: [] };
  }

  // O scroll infinito acumula ids sem limite, mas o endpoint aceita no máximo 100 por chamada.
  // Dividir aqui mantém isso como detalhe de transporte: truncar a lista faria as vagas a
  // partir da sexta página perderem o estado de candidatura em silêncio.
  const batches = await Promise.all(
    chunk(jobIds, MAX_INTERACTION_IDS).map(async (ids) => {
      const res = await axiosApi.get<unknown>('/api/jobs/feed/interactions', createAxiosConfig({ ids }));
      return jobFeedInteractionsResponseSchema.parse(res.data).appliedJobIds;
    })
  );

  return { appliedJobIds: batches.flat() };
}
