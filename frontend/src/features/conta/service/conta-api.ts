import { axiosApi, createAxiosConfig } from '@/shared/api';
import { readMessageOr, userResponseSchema, type UserResponse } from '@/shared/schema';
import {
  changeMyPasswordRequestSchema,
  updateMyProfileRequestSchema,
  type ChangeMyPasswordRequest,
  type UpdateMyProfileRequest
} from './conta-request-schema';

export async function me(): Promise<UserResponse> {
  const res = await axiosApi.get<unknown>('/api/users/me', createAxiosConfig());
  return userResponseSchema.parse(res.data);
}

export async function updateMyProfile(request: UpdateMyProfileRequest): Promise<UserResponse> {
  const body = updateMyProfileRequestSchema.parse(request);
  const res = await axiosApi.put<unknown>('/api/users/me', body, createAxiosConfig());
  return userResponseSchema.parse(res.data);
}

export async function changeMyPassword(request: ChangeMyPasswordRequest): Promise<string> {
  const body = changeMyPasswordRequestSchema.parse(request);
  const res = await axiosApi.post<unknown>('/api/users/me/change-password', body, createAxiosConfig());
  return readMessageOr(res.data, 'Senha alterada com sucesso.');
}

export async function deleteMyAccount(): Promise<void> {
  await axiosApi.delete<unknown>('/api/users/me', createAxiosConfig());
}
