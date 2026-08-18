import { newPasswordSchema, passwordConfirmationSchema } from '@/shared/auth/password-schema';
import { z } from 'zod';
import type { ResetPasswordRequest } from '../service/auth-request-schema';

export const resetPasswordFormSchema = z
  .object({
    userId: z.coerce.number().int().positive({ message: 'Link de redefinição inválido.' }),
    token: z.string().min(1, { message: 'Token inválido.' }),
    newPassword: newPasswordSchema,
    newPasswordConfirmation: passwordConfirmationSchema
  })
  .refine((data) => data.newPassword === data.newPasswordConfirmation, {
    message: 'As senhas não conferem.',
    path: ['newPasswordConfirmation']
  });

export type ResetPasswordFormValues = z.infer<typeof resetPasswordFormSchema>;

export function resetPasswordDefaultValues(userId: number, token: string): ResetPasswordFormValues {
  return { userId, token, newPassword: '', newPasswordConfirmation: '' };
}

export function resetPasswordFormToRequest(values: ResetPasswordFormValues): ResetPasswordRequest {
  return {
    userId: values.userId,
    token: values.token,
    newPassword: values.newPassword,
    newPasswordConfirmation: values.newPasswordConfirmation
  };
}
