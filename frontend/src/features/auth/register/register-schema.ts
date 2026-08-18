import { newPasswordSchema, passwordConfirmationSchema } from '@/shared/auth/password-schema';
import { z } from 'zod';
import type { RegisterRequest } from '../service/auth-request-schema';

export const registerFormSchema = z
  .object({
    username: z
      .string()
      .trim()
      .min(3, { message: 'O nome de usuário deve ter pelo menos 3 caracteres.' })
      .max(100, { message: 'O nome de usuário deve ter no máximo 100 caracteres.' }),
    email: z.email({ message: 'E-mail inválido.' }),
    password: newPasswordSchema,
    passwordConfirmation: passwordConfirmationSchema
  })
  .refine((data) => data.password === data.passwordConfirmation, {
    message: 'As senhas não conferem.',
    path: ['passwordConfirmation']
  });

export type RegisterFormValues = z.infer<typeof registerFormSchema>;

export const registerDefaultValues: RegisterFormValues = {
  username: '',
  email: '',
  password: '',
  passwordConfirmation: ''
};

export function registerFormToRequest(values: RegisterFormValues): RegisterRequest {
  return {
    username: values.username.trim(),
    email: values.email.trim(),
    password: values.password,
    passwordConfirmation: values.passwordConfirmation
  };
}
