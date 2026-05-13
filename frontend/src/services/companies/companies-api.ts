import { axiosApi, createAxiosConfig } from '../axios';
import { companiesListResponseSchema, companySchema, type CompaniesListResponseDto } from './companies-schema';

export async function listCompanies(
  token: string,
  params?: { page?: number; size?: number; orderBy?: string; isDeleted?: boolean; isActive?: boolean }
): Promise<CompaniesListResponseDto> {
  const res = await axiosApi.get<unknown>('/api/companies', createAxiosConfig(token, params));
  return companiesListResponseSchema.parse(res.data);
}

export async function getCompany(token: string, id: number) {
  const res = await axiosApi.get<unknown>(`/api/companies/${id}`, createAxiosConfig(token));
  return companySchema.parse(res.data);
}

export async function createCompany(token: string, dto: unknown): Promise<string> {
  const res = await axiosApi.post<string>('/api/companies', dto, createAxiosConfig(token));
  return res.data;
}

export async function updateCompany(token: string, id: number, dto: unknown) {
  const res = await axiosApi.put<unknown>(`/api/companies/${id}`, dto, createAxiosConfig(token));
  return res.data;
}

export async function deleteCompany(token: string, id: number) {
  const res = await axiosApi.delete<unknown>(`/api/companies/${id}`, createAxiosConfig(token));
  return res.data;
}
