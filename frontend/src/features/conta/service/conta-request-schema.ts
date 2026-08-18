import { newPasswordSchema } from '@/shared/auth/password-schema';
import { z } from 'zod';

export const updateMyProfileRequestSchema = z
  .object({
    username: z.string().trim().min(3).max(100).nullable().optional(),
    email: z.email().nullable().optional(),
    phoneNumber: z.string().nullable().optional()
  })
  .refine((body) => Boolean(body.username || body.email || body.phoneNumber !== undefined), {
    message: 'Informe ao menos um campo para atualizar (email, username ou phoneNumber).'
  });

export const changeMyPasswordRequestSchema = z.object({
  currentPassword: z.string().min(1),
  newPassword: newPasswordSchema,
  newPasswordConfirmation: z.string().min(1)
});

export type UpdateMyProfileRequest = z.infer<typeof updateMyProfileRequestSchema>;
export type ChangeMyPasswordRequest = z.infer<typeof changeMyPasswordRequestSchema>;
