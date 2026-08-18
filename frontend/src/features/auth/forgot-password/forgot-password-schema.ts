import { z } from 'zod';
import type { ForgotPasswordRequest } from '../service/auth-request-schema';

export const forgotPasswordFormSchema = z.object({
  email: z.email({ message: 'E-mail inválido.' })
});

export type ForgotPasswordFormValues = z.infer<typeof forgotPasswordFormSchema>;

export const forgotPasswordDefaultValues: ForgotPasswordFormValues = { email: '' };

export function forgotPasswordFormToRequest(values: ForgotPasswordFormValues): ForgotPasswordRequest {
  return { email: values.email.trim() };
}
