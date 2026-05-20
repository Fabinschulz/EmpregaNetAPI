import { axiosApi, createAxiosConfig } from '../axios';
import type {
    JobApplicationsAdminListQueryParams,
    JobApplicationsListQueryParams
} from '../shared';
import { jobApplicationsListResponseSchema, type JobApplicationsListResponseDto } from './job-applications-schema';

export async function applyToJob(token: string, dto: unknown): Promise<string> {
  const res = await axiosApi.post<string>('/api/jobapplications', dto, createAxiosConfig(token));
  return res.data;
}

export async function listMine(
  token: string,
  params?: JobApplicationsListQueryParams
): Promise<JobApplicationsListResponseDto> {
  const res = await axiosApi.get<unknown>('/api/jobapplications/mine', createAxiosConfig(token, params));
  return jobApplicationsListResponseSchema.parse(res.data);
}

export async function listAll(
  token: string,
  params?: JobApplicationsAdminListQueryParams
): Promise<JobApplicationsListResponseDto> {
  const res = await axiosApi.get<unknown>('/api/jobapplications', createAxiosConfig(token, params));
  return jobApplicationsListResponseSchema.parse(res.data);
}

export async function listByJob(
  token: string,
  jobId: number,
  params?: JobApplicationsListQueryParams
): Promise<JobApplicationsListResponseDto> {
  const res = await axiosApi.get<unknown>(`/api/jobapplications/job/${jobId}`, createAxiosConfig(token, params));
  return jobApplicationsListResponseSchema.parse(res.data);
}

export async function changeStatus(token: string, id: number, dto: unknown): Promise<unknown> {
  const res = await axiosApi.put<unknown>(`/api/jobapplications/${id}`, dto, createAxiosConfig(token));
  return res.data;
}

export async function deleteApplication(token: string, id: number): Promise<void> {
  await axiosApi.delete(`/api/jobapplications/${id}`, createAxiosConfig(token));
}
