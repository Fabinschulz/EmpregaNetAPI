import { axiosApi, createAxiosConfig } from '@/shared/api';
import { type CandidatesListQueryParams } from '@/shared/schema';
import { candidateDetailResponseSchema, type CandidateDetailResponse } from './candidate-detail-response-schema';
import { candidatesListResponseSchema, type CandidatesListResponse } from './candidates-response-schema';

export async function listCandidates(params?: CandidatesListQueryParams): Promise<CandidatesListResponse> {
  const res = await axiosApi.get<unknown>('/api/candidates', createAxiosConfig(params));
  return candidatesListResponseSchema.parse(res.data);
}

export async function getCandidate(id: number): Promise<CandidateDetailResponse> {
  const res = await axiosApi.get<unknown>(`/api/candidates/${id}`, createAxiosConfig());
  return candidateDetailResponseSchema.parse(res.data);
}
