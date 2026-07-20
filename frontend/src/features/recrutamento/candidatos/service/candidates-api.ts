import { axiosApi, createAxiosConfig } from '@/shared/api';
import { userSchema } from '@/shared/auth';
import type { CandidatesListQueryParams } from '@/shared/schema';
import { candidatesListResponseSchema, type CandidatesListResponseDto } from './candidates-schema';

export async function listCandidates(params?: CandidatesListQueryParams): Promise<CandidatesListResponseDto> {
  const res = await axiosApi.get<unknown>('/api/candidates', createAxiosConfig(params));
  return candidatesListResponseSchema.parse(res.data);
}

export async function getCandidate(id: number) {
  const res = await axiosApi.get<unknown>(`/api/candidates/${id}`, createAxiosConfig());
  return userSchema.parse(res.data);
}
