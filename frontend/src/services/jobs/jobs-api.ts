import type { JobsListQueryParams } from '@/shared';
import { axiosApi, createAxiosConfig } from '../axios';
import { jobFormSchema, jobSchema, jobsListResponseSchema, type JobDto, type JobsListResponseDto } from './jobs-schema';

export async function listJobs(params?: JobsListQueryParams): Promise<JobsListResponseDto> {
  const res = await axiosApi.get<unknown>('/api/jobs', { params });
  return jobsListResponseSchema.parse(res.data);
}

export async function getJob(id: number): Promise<JobDto> {
  const res = await axiosApi.get<unknown>(`/api/jobs/${id}`);
  return jobSchema.parse(res.data);
}

export async function createJob(dto: unknown): Promise<string> {
  const body = jobFormSchema.parse(dto);
  const res = await axiosApi.post<string>('/api/jobs', body, createAxiosConfig());
  return res.data;
}

export async function updateJob(id: number, dto: unknown): Promise<unknown> {
  const body = jobFormSchema.parse(dto);
  const res = await axiosApi.put<unknown>(`/api/jobs/${id}`, body, createAxiosConfig());
  return res.data;
}

export async function closeJob(id: number): Promise<string> {
  const res = await axiosApi.put<string>(`/api/jobs/${id}/close`, undefined, createAxiosConfig());
  return res.data;
}

export async function deleteJob(id: number): Promise<void> {
  await axiosApi.delete(`/api/jobs/${id}`, createAxiosConfig());
}
