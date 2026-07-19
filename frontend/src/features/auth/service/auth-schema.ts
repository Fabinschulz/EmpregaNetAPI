import { z } from 'zod';

export const registerSchema = z.object({
  username: z
    .string()
    .min(3, { message: 'O nome de usuário deve ter pelo menos 3 caracteres.' })
    .max(64, { message: 'O nome de usuário deve ter no máximo 64 caracteres.' }),
  email: z.string().email({ message: 'E-mail inválido.' }),
  password: z.string().min(8, { message: 'A senha deve ter pelo menos 8 caracteres.' }),
  passwordConfirmation: z.string().min(8, { message: 'A confirmação da senha deve ter pelo menos 8 caracteres.' })
});

export const registerFormSchema = registerSchema.refine((data) => data.password === data.passwordConfirmation, {
  message: 'As senhas não conferem.',
  path: ['passwordConfirmation']
});

export const loginSchema = z.object({
  login: z.email({ message: 'E-mail inválido.' }),
  password: z.string().min(1, { message: 'A senha é obrigatória.' })
});

export const forgotPasswordSchema = z.object({
  email: z.email({ message: 'E-mail inválido.' })
});

export const resetPasswordFormSchema = z
  .object({
    userId: z.coerce.number().int().positive({ message: 'Link de redefinição inválido.' }),
    token: z.string().min(1, { message: 'Token inválido.' }),
    newPassword: z.string().min(8, { message: 'A senha deve ter pelo menos 8 caracteres.' }),
    newPasswordConfirmation: z.string().min(8, { message: 'Confirme a nova senha.' })
  })
  .refine((data) => data.newPassword === data.newPasswordConfirmation, {
    message: 'As senhas não conferem.',
    path: ['newPasswordConfirmation']
  });

export const confirmEmailSchema = z.object({
  userId: z.coerce.number().int().positive({ message: 'Link de confirmação inválido.' }),
  token: z.string().min(1, { message: 'Token inválido.' })
});

export const loginWithGoogleSchema = z.object({
  idToken: z.string().min(1, { message: 'Token do Google inválido.' })
});

export const resendEmailConfirmationSchema = z.object({
  email: z.email({ message: 'E-mail inválido.' })
});

/** Respostas `{ message }` dos fluxos de conta (recuperação/confirmação). */
export const authMessageResponseSchema = z.object({
  message: z.string()
});

export type RegisterDto = z.infer<typeof registerSchema>;
export type LoginDto = z.infer<typeof loginSchema>;
export type ForgotPasswordDto = z.infer<typeof forgotPasswordSchema>;
export type ResetPasswordFormValues = z.infer<typeof resetPasswordFormSchema>;
export type ConfirmEmailDto = z.infer<typeof confirmEmailSchema>;
export type LoginWithGoogleDto = z.infer<typeof loginWithGoogleSchema>;
export type ResendEmailConfirmationDto = z.infer<typeof resendEmailConfirmationSchema>;
