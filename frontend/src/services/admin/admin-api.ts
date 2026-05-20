import { axiosApi, createAxiosConfig } from '../axios';
import type { AdminUsersListQueryParams } from '../shared';
import { userSchema } from '../users/users-schema';
import { adminUsersListResponseSchema, type AdminUsersListResponseDto } from './admin-schema';

export async function listAdminUsers(
  token: string,
  params?: AdminUsersListQueryParams
): Promise<AdminUsersListResponseDto> {
  const res = await axiosApi.get<unknown>('/api/admin', createAxiosConfig(token, params));
  return adminUsersListResponseSchema.parse(res.data);
}

export async function getAdminUser(token: string, id: number) {
  const res = await axiosApi.get<unknown>(`/api/admin/${id}`, createAxiosConfig(token));
  return userSchema.parse(res.data);
}

export async function updateAdminUser(token: string, id: number, dto: unknown) {
  const res = await axiosApi.put<unknown>(`/api/admin/${id}`, dto, createAxiosConfig(token));
  return res.data;
}

export async function deleteAdminUser(token: string, id: number) {
  const res = await axiosApi.delete<unknown>(`/api/admin/${id}`, createAxiosConfig(token));
  return res.data;
}
