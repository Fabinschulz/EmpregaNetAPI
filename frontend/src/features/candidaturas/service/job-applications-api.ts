import { axiosApi, createAxiosConfig } from '@/shared/api';
import {
    createdIdResponseSchema,
    type CreatedId,
    type JobApplicationsAdminListQueryParams,
    type JobApplicationsListQueryParams
} from '@/shared/schema';
import {
    applyToJobRequestSchema,
    changeApplicationStatusRequestSchema,
    type ApplyToJobRequest,
    type ChangeApplicationStatusRequest
} from './job-applications-request-schema';
import {
    jobApplicationResponseSchema,
    jobApplicationsListResponseSchema,
    type JobApplicationResponse,
    type JobApplicationsListResponse
} from './job-applications-response-schema';

export async function applyToJob(request: ApplyToJobRequest): Promise<CreatedId> {
  const body = applyToJobRequestSchema.parse(request);
  const res = await axiosApi.post<unknown>('/api/jobapplications', body, createAxiosConfig());
  return createdIdResponseSchema.parse(res.data);
}

export async function listMine(params?: JobApplicationsListQueryParams): Promise<JobApplicationsListResponse> {
  const res = await axiosApi.get<unknown>('/api/jobapplications/mine', createAxiosConfig(params));
  return jobApplicationsListResponseSchema.parse(res.data);
}

export async function listAll(params?: JobApplicationsAdminListQueryParams): Promise<JobApplicationsListResponse> {
  const res = await axiosApi.get<unknown>('/api/jobapplications', createAxiosConfig(params));
  return jobApplicationsListResponseSchema.parse(res.data);
}

export async function listByJob(
  jobId: number,
  params?: JobApplicationsListQueryParams
): Promise<JobApplicationsListResponse> {
  const res = await axiosApi.get<unknown>(`/api/jobapplications/job/${jobId}`, createAxiosConfig(params));
  return jobApplicationsListResponseSchema.parse(res.data);
}

export async function changeStatus(
  id: number,
  request: ChangeApplicationStatusRequest
): Promise<JobApplicationResponse> {
  const body = changeApplicationStatusRequestSchema.parse(request);
  const res = await axiosApi.put<unknown>(`/api/jobapplications/${id}`, body, createAxiosConfig());
  return jobApplicationResponseSchema.parse(res.data);
}

export async function deleteApplication(id: number): Promise<void> {
  await axiosApi.delete(`/api/jobapplications/${id}`, createAxiosConfig());
}

export async function cancelJobApplication(id: number): Promise<JobApplicationResponse> {
  const res = await axiosApi.put<unknown>(`/api/jobapplications/${id}/cancel`, undefined, createAxiosConfig());
  return jobApplicationResponseSchema.parse(res.data);
}
