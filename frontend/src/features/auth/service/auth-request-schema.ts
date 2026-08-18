import { existingPasswordSchema, newPasswordSchema } from '@/shared/auth/password-schema';
import { z } from 'zod';

export const loginRequestSchema = z.object({
  login: z.email({ message: 'E-mail inválido.' }),
  password: existingPasswordSchema
});

export const registerRequestSchema = z.object({
  username: z
    .string()
    .trim()
    .min(3, { message: 'O nome de usuário deve ter pelo menos 3 caracteres.' })
    .max(100, { message: 'O nome de usuário deve ter no máximo 100 caracteres.' }),
  email: z.email({ message: 'E-mail inválido.' }),
  password: newPasswordSchema,
  passwordConfirmation: z.string().min(1),
  phoneNumber: z.string().nullable().optional()
});

export const forgotPasswordRequestSchema = z.object({
  email: z.email({ message: 'E-mail inválido.' })
});

export const resetPasswordRequestSchema = z.object({
  userId: z.number().int().positive(),
  token: z.string().min(1),
  newPassword: newPasswordSchema,
  newPasswordConfirmation: z.string().min(1)
});

export const confirmEmailRequestSchema = z.object({
  userId: z.number().int().positive(),
  token: z.string().min(1)
});

export const loginWithGoogleRequestSchema = z.object({
  idToken: z.string().min(1, { message: 'Token do Google inválido.' })
});

export const resendEmailConfirmationRequestSchema = z.object({
  email: z.email({ message: 'E-mail inválido.' })
});

export type LoginRequest = z.infer<typeof loginRequestSchema>;
export type RegisterRequest = z.infer<typeof registerRequestSchema>;
export type ForgotPasswordRequest = z.infer<typeof forgotPasswordRequestSchema>;
export type ResetPasswordRequest = z.infer<typeof resetPasswordRequestSchema>;
export type ConfirmEmailRequest = z.infer<typeof confirmEmailRequestSchema>;
export type LoginWithGoogleRequest = z.infer<typeof loginWithGoogleRequestSchema>;
export type ResendEmailConfirmationRequest = z.infer<typeof resendEmailConfirmationRequestSchema>;
