import { axiosApi, createAxiosConfig } from '@/shared/api';
import { createdIdResponseSchema, type CompaniesListQueryParams, type CreatedId } from '@/shared/schema';
import { companyRequestSchema, type CompanyRequest } from './companies-request-schema';
import {
  companiesListResponseSchema,
  companyResponseSchema,
  type CompaniesListResponse,
  type CompanyResponse
} from './companies-response-schema';

export async function listCompanies(params?: CompaniesListQueryParams): Promise<CompaniesListResponse> {
  const res = await axiosApi.get<unknown>('/api/companies', createAxiosConfig(params));
  return companiesListResponseSchema.parse(res.data);
}

export async function getCompany(id: number): Promise<CompanyResponse> {
  const res = await axiosApi.get<unknown>(`/api/companies/${id}`, createAxiosConfig());
  return companyResponseSchema.parse(res.data);
}

export async function createCompany(request: CompanyRequest): Promise<CreatedId> {
  const body = companyRequestSchema.parse(request);
  const res = await axiosApi.post<unknown>('/api/companies', body, createAxiosConfig());
  return createdIdResponseSchema.parse(res.data);
}

export async function updateCompany(id: number, request: CompanyRequest): Promise<CompanyResponse> {
  const body = companyRequestSchema.parse(request);
  const res = await axiosApi.put<unknown>(`/api/companies/${id}`, body, createAxiosConfig());
  return companyResponseSchema.parse(res.data);
}

export async function deleteCompany(id: number): Promise<void> {
  await axiosApi.delete<unknown>(`/api/companies/${id}`, createAxiosConfig());
}
