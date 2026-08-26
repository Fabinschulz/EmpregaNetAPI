import { newPasswordSchema, passwordConfirmationSchema } from '@/shared/auth/password-schema';
import { isValidCpf, onlyDigits } from '@/shared/utils';
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
    cpf: z
      .string()
      .trim()
      .min(1, { message: 'Informe o seu CPF.' })
      .refine(isValidCpf, { message: 'CPF inválido: verifique os dígitos informados.' }),
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
  cpf: '',
  password: '',
  passwordConfirmation: ''
};

export function registerFormToRequest(values: RegisterFormValues): RegisterRequest {
  return {
    username: values.username.trim(),
    email: values.email.trim(),
    cpf: onlyDigits(values.cpf),
    password: values.password,
    passwordConfirmation: values.passwordConfirmation
  };
}
