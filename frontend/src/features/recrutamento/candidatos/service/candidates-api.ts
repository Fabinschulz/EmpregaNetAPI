import { axiosApi, createAxiosConfig } from '@/shared/api';
import { userResponseSchema, type CandidatesListQueryParams, type UserResponse } from '@/shared/schema';
import { candidatesListResponseSchema, type CandidatesListResponse } from './candidates-response-schema';

export async function listCandidates(params?: CandidatesListQueryParams): Promise<CandidatesListResponse> {
  const res = await axiosApi.get<unknown>('/api/candidates', createAxiosConfig(params));
  return candidatesListResponseSchema.parse(res.data);
}

export async function getCandidate(id: number): Promise<UserResponse> {
  const res = await axiosApi.get<unknown>(`/api/candidates/${id}`, createAxiosConfig());
  return userResponseSchema.parse(res.data);
}
