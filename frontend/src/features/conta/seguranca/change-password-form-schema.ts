import { newPasswordSchema, passwordConfirmationSchema } from '@/shared/auth/password-schema';
import { z } from 'zod';
import type { ChangeMyPasswordRequest } from '../service/conta-request-schema';

export const changeMyPasswordFormSchema = z
  .object({
    currentPassword: z.string().min(1, { message: 'Informe a senha atual.' }),
    newPassword: newPasswordSchema,
    newPasswordConfirmation: passwordConfirmationSchema
  })
  .refine((data) => data.newPassword === data.newPasswordConfirmation, {
    message: 'As senhas não conferem.',
    path: ['newPasswordConfirmation']
  });

export type ChangeMyPasswordFormValues = z.infer<typeof changeMyPasswordFormSchema>;

export const defaultChangeMyPasswordForm: ChangeMyPasswordFormValues = {
  currentPassword: '',
  newPassword: '',
  newPasswordConfirmation: ''
};

export function changeMyPasswordFormToRequest(values: ChangeMyPasswordFormValues): ChangeMyPasswordRequest {
  return {
    currentPassword: values.currentPassword,
    newPassword: values.newPassword,
    newPasswordConfirmation: values.newPasswordConfirmation
  };
}
