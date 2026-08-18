import { axiosApi, createAxiosConfig } from '@/shared/api';
import { userResponseSchema, type AdminUsersListQueryParams, type UserResponse } from '@/shared/schema';
import { updateAdminUserRequestSchema, type UpdateAdminUserRequest } from './admin-request-schema';
import { adminUsersListResponseSchema, type AdminUsersListResponse } from './admin-response-schema';

export async function listAdminUsers(params?: AdminUsersListQueryParams): Promise<AdminUsersListResponse> {
  const res = await axiosApi.get<unknown>('/api/admin', createAxiosConfig(params));
  return adminUsersListResponseSchema.parse(res.data);
}

export async function getAdminUser(id: number): Promise<UserResponse> {
  const res = await axiosApi.get<unknown>(`/api/admin/${id}`, createAxiosConfig());
  return userResponseSchema.parse(res.data);
}

export async function updateAdminUser(id: number, request: UpdateAdminUserRequest): Promise<UserResponse> {
  const body = updateAdminUserRequestSchema.parse(request);
  const res = await axiosApi.put<unknown>(`/api/admin/${id}`, body, createAxiosConfig());
  return userResponseSchema.parse(res.data);
}

export async function deleteAdminUser(id: number): Promise<void> {
  await axiosApi.delete<unknown>(`/api/admin/${id}`, createAxiosConfig());
}
