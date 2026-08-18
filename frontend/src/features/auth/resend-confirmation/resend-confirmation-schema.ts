import { z } from 'zod';
import type { ResendEmailConfirmationRequest } from '../service/auth-request-schema';

export const resendConfirmationFormSchema = z.object({
  email: z.email({ message: 'E-mail inválido.' })
});

export type ResendConfirmationFormValues = z.infer<typeof resendConfirmationFormSchema>;

export const resendConfirmationDefaultValues: ResendConfirmationFormValues = { email: '' };

export function resendConfirmationFormToRequest(values: ResendConfirmationFormValues): ResendEmailConfirmationRequest {
  return { email: values.email.trim() };
}
