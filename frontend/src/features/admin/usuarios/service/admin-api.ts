import { axiosApi, createAxiosConfig, UserDto, userSchema, type AdminUsersListQueryParams } from '@/shared';
import { adminUsersListResponseSchema, type AdminUsersListResponseDto } from './admin-schema';

export async function listAdminUsers(params?: AdminUsersListQueryParams): Promise<AdminUsersListResponseDto> {
  const res = await axiosApi.get<unknown>('/api/admin', createAxiosConfig(params));
  return adminUsersListResponseSchema.parse(res.data);
}

export async function getAdminUser(id: number): Promise<UserDto> {
  const res = await axiosApi.get<unknown>(`/api/admin/${id}`, createAxiosConfig());
  return userSchema.parse(res.data);
}

export async function updateAdminUser(id: number, dto: unknown) {
  const res = await axiosApi.put<unknown>(`/api/admin/${id}`, dto, createAxiosConfig());
  return res.data;
}

export async function deleteAdminUser(id: number) {
  const res = await axiosApi.delete<unknown>(`/api/admin/${id}`, createAxiosConfig());
  return res.data;
}
