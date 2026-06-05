import { axiosApi, createAxiosConfig } from '../axios';
import type { JobsListQueryParams } from '../shared';
import { jobFormSchema, jobSchema, jobsListResponseSchema, type JobDto, type JobsListResponseDto } from './jobs-schema';

export async function listJobs(params?: JobsListQueryParams): Promise<JobsListResponseDto> {
  const res = await axiosApi.get<unknown>('/api/jobs', { params });
  return jobsListResponseSchema.parse(res.data);
}

export async function getJob(id: number): Promise<JobDto> {
  const res = await axiosApi.get<unknown>(`/api/jobs/${id}`);
  return jobSchema.parse(res.data);
}

export async function createJob(token: string, dto: unknown): Promise<string> {
  const body = jobFormSchema.parse(dto);
  const res = await axiosApi.post<string>('/api/jobs', body, createAxiosConfig(token));
  return res.data;
}

export async function updateJob(token: string, id: number, dto: unknown): Promise<unknown> {
  const body = jobFormSchema.parse(dto);
  const res = await axiosApi.put<unknown>(`/api/jobs/${id}`, body, createAxiosConfig(token));
  return res.data;
}

export async function closeJob(token: string, id: number): Promise<string> {
  const res = await axiosApi.put<string>(`/api/jobs/${id}/close`, undefined, createAxiosConfig(token));
  return res.data;
}

export async function deleteJob(token: string, id: number): Promise<void> {
  await axiosApi.delete(`/api/jobs/${id}`, createAxiosConfig(token));
}
