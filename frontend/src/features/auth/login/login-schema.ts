import { existingPasswordSchema } from '@/shared/auth/password-schema';
import { isValidLoginIdentifier, onlyDigits } from '@/shared/utils';
import { z } from 'zod';
import type { LoginRequest } from '../service/auth-request-schema';

export const loginFormSchema = z.object({
  identifier: z
    .string()
    .trim()
    .min(1, { message: 'Informe o seu CPF ou e-mail.' })
    .refine(isValidLoginIdentifier, { message: 'Informe um CPF ou e-mail válido.' }),
  password: existingPasswordSchema
});

export type LoginFormValues = z.infer<typeof loginFormSchema>;

export const loginDefaultValues: LoginFormValues = {
  identifier: '',
  password: ''
};

export function loginFormToRequest(values: LoginFormValues): LoginRequest {
  const identifier = values.identifier.trim();

  return {
    identifier: identifier.includes('@') ? identifier : onlyDigits(identifier),
    password: values.password
  };
}
