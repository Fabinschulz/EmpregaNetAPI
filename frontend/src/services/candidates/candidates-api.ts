import { axiosApi, createAxiosConfig } from "../axios-api";
import { candidatesListResponseSchema, type CandidatesListResponseDto } from "./candidates-schema";
import { userSchema } from "../users/users-schema";

export async function listCandidates(
  token: string,
  params?: { page?: number; size?: number; orderBy?: string }
): Promise<CandidatesListResponseDto> {
  const res = await axiosApi.get<unknown>("/api/candidates", createAxiosConfig(token, params));
  return candidatesListResponseSchema.parse(res.data);
}

export async function getCandidate(token: string, id: number) {
  const res = await axiosApi.get<unknown>(`/api/candidates/${id}`, createAxiosConfig(token));
  return userSchema.parse(res.data);
}
