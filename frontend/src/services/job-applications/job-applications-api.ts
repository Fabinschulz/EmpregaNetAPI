import { axiosApi, createAxiosConfig } from '../axios';
import type { JobApplicationsAdminListQueryParams, JobApplicationsListQueryParams } from '../shared';
import {
  applyToJobSchema,
  changeApplicationStatusSchema,
  jobApplicationsListResponseSchema,
  type JobApplicationsListResponseDto
} from './job-applications-schema';

export async function applyToJob(dto: unknown): Promise<string> {
  const body = applyToJobSchema.parse(dto);
  const res = await axiosApi.post<string>('/api/jobapplications', body, createAxiosConfig());
  return res.data;
}

export async function listMine(
  params?: JobApplicationsListQueryParams
): Promise<JobApplicationsListResponseDto> {
  const res = await axiosApi.get<unknown>('/api/jobapplications/mine', createAxiosConfig(params));
  return jobApplicationsListResponseSchema.parse(res.data);
}

export async function listAll(
  params?: JobApplicationsAdminListQueryParams
): Promise<JobApplicationsListResponseDto> {
  const res = await axiosApi.get<unknown>('/api/jobapplications', createAxiosConfig(params));
  return jobApplicationsListResponseSchema.parse(res.data);
}

export async function listByJob(
  jobId: number,
  params?: JobApplicationsListQueryParams
): Promise<JobApplicationsListResponseDto> {
  const res = await axiosApi.get<unknown>(`/api/jobapplications/job/${jobId}`, createAxiosConfig(params));
  return jobApplicationsListResponseSchema.parse(res.data);
}

export async function changeStatus(id: number, dto: unknown): Promise<unknown> {
  const body = changeApplicationStatusSchema.parse(dto);
  const res = await axiosApi.put<unknown>(`/api/jobapplications/${id}`, body, createAxiosConfig());
  return res.data;
}

export async function deleteApplication(id: number): Promise<void> {
  await axiosApi.delete(`/api/jobapplications/${id}`, createAxiosConfig());
}
