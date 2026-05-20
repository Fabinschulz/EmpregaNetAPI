import { axiosApi, createAxiosConfig } from '../axios';
import type { CandidatesListQueryParams } from '../shared';
import { userSchema } from '../users/users-schema';
import { candidatesListResponseSchema, type CandidatesListResponseDto } from './candidates-schema';

export async function listCandidates(
  token: string,
  params?: CandidatesListQueryParams
): Promise<CandidatesListResponseDto> {
  const res = await axiosApi.get<unknown>('/api/candidates', createAxiosConfig(token, params));
  return candidatesListResponseSchema.parse(res.data);
}

export async function getCandidate(token: string, id: number) {
  const res = await axiosApi.get<unknown>(`/api/candidates/${id}`, createAxiosConfig(token));
  return userSchema.parse(res.data);
}
