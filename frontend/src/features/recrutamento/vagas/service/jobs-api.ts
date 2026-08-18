import { axiosApi, createAxiosConfig } from '@/shared/api';
import { createdIdResponseSchema, type CreatedId, type JobsListQueryParams } from '@/shared/schema';
import { jobRequestSchema, type JobRequest } from './jobs-request-schema';
import {
  companyOptionsResponseSchema,
  jobResponseSchema,
  jobsListResponseSchema,
  type CompanyOption,
  type JobResponse,
  type JobsListResponse
} from './jobs-response-schema';

export async function listJobs(params?: JobsListQueryParams): Promise<JobsListResponse> {
  const res = await axiosApi.get<unknown>('/api/jobs', { params });
  return jobsListResponseSchema.parse(res.data);
}

export async function getJob(id: number): Promise<JobResponse> {
  const res = await axiosApi.get<unknown>(`/api/jobs/${id}`);
  return jobResponseSchema.parse(res.data);
}

export async function listSelectableCompanies(): Promise<CompanyOption[]> {
  const res = await axiosApi.get<unknown>('/api/jobs/selectable-companies', createAxiosConfig());
  return companyOptionsResponseSchema.parse(res.data);
}

export async function createJob(request: JobRequest): Promise<CreatedId> {
  const body = jobRequestSchema.parse(request);
  const res = await axiosApi.post<unknown>('/api/jobs', body, createAxiosConfig());
  return createdIdResponseSchema.parse(res.data);
}

export async function updateJob(id: number, request: JobRequest): Promise<JobResponse> {
  const body = jobRequestSchema.parse(request);
  const res = await axiosApi.put<unknown>(`/api/jobs/${id}`, body, createAxiosConfig());
  return jobResponseSchema.parse(res.data);
}

export async function closeJob(id: number): Promise<string> {
  const res = await axiosApi.put<string>(`/api/jobs/${id}/close`, undefined, createAxiosConfig());
  return res.data;
}

export async function deleteJob(id: number): Promise<void> {
  await axiosApi.delete(`/api/jobs/${id}`, createAxiosConfig());
}
