import type { UserResponse } from '@/shared/schema';
import { isValidBrazilCellPhone, maskBrazilPhone, onlyDigits } from '@/utils';
import { z } from 'zod';
import type { UpdateMyProfileRequest } from '../service/conta-request-schema';

export const profileFormSchema = z.object({
  username: z
    .string()
    .trim()
    .min(3, { message: 'O nome de usuário deve ter pelo menos 3 caracteres.' })
    .max(100, { message: 'O nome de usuário deve ter no máximo 100 caracteres.' }),
  email: z.email({ message: 'E-mail inválido.' }),
  phoneNumber: z
    .string()
    .optional()
    .nullable()
    .refine((value) => !value || value.trim() === '' || isValidBrazilCellPhone(value), {
      message: 'Celular inválido. Informe DDD + 9 dígitos, ex.: (11) 98765-4321.'
    })
});

export type ProfileFormValues = z.infer<typeof profileFormSchema>;

export function profileFormValuesFromResponse(user: UserResponse): ProfileFormValues {
  return {
    username: user.username,
    email: user.email,
    phoneNumber: user.phoneNumber ? maskBrazilPhone(user.phoneNumber) : ''
  };
}

export function profileFormToRequest(values: ProfileFormValues): UpdateMyProfileRequest {
  return {
    username: values.username.trim(),
    email: values.email.trim(),
    phoneNumber: onlyDigits(values.phoneNumber) || null
  };
}
