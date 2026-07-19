import { axiosApi, createAxiosConfig, onlyDigits, userSchema, type UserDto } from '@/shared';
import { z } from 'zod';
import { changeMyPasswordFormSchema, profileFormSchema, type ChangeMyPasswordFormValues, type ProfileFormValues } from './conta-schema';

const messageResponseSchema = z.object({ message: z.string() });

export async function me(): Promise<UserDto> {
  const res = await axiosApi.get<unknown>('/api/users/me', createAxiosConfig());
  return userSchema.parse(res.data);
}

/** Atualiza os dados da própria conta (username, e-mail, telefone). */
export async function updateMyProfile(dto: ProfileFormValues): Promise<UserDto> {
  const values = profileFormSchema.parse(dto);
  const digits = onlyDigits(values.phoneNumber);
  const body = {
    userName: values.username,
    email: values.email,
    phoneNumber: digits || null
  };
  const res = await axiosApi.put<unknown>('/api/users/me', body, createAxiosConfig());
  return userSchema.parse(res.data);
}

/** Altera a própria senha (exige a senha atual). Revoga as sessões abertas no servidor. */
export async function changeMyPassword(dto: ChangeMyPasswordFormValues): Promise<string> {
  const body = changeMyPasswordFormSchema.parse(dto);
  const res = await axiosApi.post<unknown>('/api/users/me/change-password', body, createAxiosConfig());
  const parsed = messageResponseSchema.safeParse(res.data);
  if (parsed.success) return parsed.data.message;
  return 'Senha alterada com sucesso.';
}

/** Encerra a própria conta (exclusão lógica; o registro permanece para auditoria). */
export async function deleteMyAccount(): Promise<void> {
  await axiosApi.delete<unknown>('/api/users/me', createAxiosConfig());
}
