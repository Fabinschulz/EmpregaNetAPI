import { existingPasswordSchema } from '@/shared/auth/password-schema';
import { z } from 'zod';
import type { LoginRequest } from '../service/auth-request-schema';

export const loginFormSchema = z.object({
  login: z.email({ message: 'E-mail inválido.' }),
  password: existingPasswordSchema
});

export type LoginFormValues = z.infer<typeof loginFormSchema>;

export const loginDefaultValues: LoginFormValues = {
  login: '',
  password: ''
};

export function loginFormToRequest(values: LoginFormValues): LoginRequest {
  return { login: values.login.trim(), password: values.password };
}
