import type { CompaniesListQueryParams } from '@/shared';
import { axiosApi, createAxiosConfig } from '../axios';
import {
  companiesListResponseSchema,
  CompanyDto,
  companyFormSchema,
  companySchema,
  type CompaniesListResponseDto
} from './companies-schema';

export async function listCompanies(params?: CompaniesListQueryParams): Promise<CompaniesListResponseDto> {
  const res = await axiosApi.get<unknown>('/api/companies', createAxiosConfig(params));
  return companiesListResponseSchema.parse(res.data);
}

export async function getCompany(id: number): Promise<CompanyDto> {
  const res = await axiosApi.get<unknown>(`/api/companies/${id}`, createAxiosConfig());
  return companySchema.parse(res.data);
}

export async function createCompany(dto: unknown): Promise<string> {
  const body = companyFormSchema.parse(dto);
  const res = await axiosApi.post<string>('/api/companies', body, createAxiosConfig());
  return res.data;
}

export async function updateCompany(id: number, dto: unknown) {
  const body = companyFormSchema.parse(dto);
  const res = await axiosApi.put<unknown>(`/api/companies/${id}`, body, createAxiosConfig());
  return res.data;
}

export async function deleteCompany(id: number) {
  const res = await axiosApi.delete<unknown>(`/api/companies/${id}`, createAxiosConfig());
  return res.data;
}
