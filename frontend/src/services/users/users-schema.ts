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

export const refreshTokenSchema = z.object({
  refreshToken: z.string().min(1, { message: 'O token de atualização é obrigatório.' })
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

export const forgotPasswordResponseSchema = z.object({
  message: z.string()
});

export const confirmEmailResponseSchema = z.object({
  message: z.string()
});

export const resetPasswordResponseSchema = z.object({
  message: z.string()
});

export const userClaimSchema = z.object({
  value: z.string(),
  type: z.string()
});

export const userTokenSchema = z.object({
  id: z.number().int(),
  username: z.string(),
  email: z.email({ message: 'E-mail inválido.' }),
  roles: z.array(z.string()).default([]),
  claims: z.array(userClaimSchema)
});

export const userLoggedSchema = z.object({
  accessToken: z.string().min(1, { message: 'O token de acesso é obrigatório.' }),
  expiresIn: z.number(),
  userToken: userTokenSchema,
  permissions: z.record(z.string(), z.array(z.string())).optional(),
  permissionCodes: z.array(z.string()).optional(),
  key: z.string().optional(),
  refreshToken: z.string().nullable().optional()
});

export const userSchema = z.object({
  id: z.number().int(),
  username: z
    .string()
    .min(3, { message: 'O nome de usuário deve ter pelo menos 3 caracteres.' })
    .max(64, { message: 'O nome de usuário deve ter no máximo 64 caracteres.' }),
  email: z.string().email({ message: 'E-mail inválido.' }),
  phoneNumber: z.string().nullable().optional(),
  userType: z.string().nullable().optional(),
  roles: z.array(z.string()).default([]),
  isDeleted: z.boolean().optional()
});

export type RegisterDto = z.infer<typeof registerSchema>;
export type LoginDto = z.infer<typeof loginSchema>;
export type ForgotPasswordDto = z.infer<typeof forgotPasswordSchema>;
export type ResetPasswordFormValues = z.infer<typeof resetPasswordFormSchema>;
export type ConfirmEmailDto = z.infer<typeof confirmEmailSchema>;
export type LoginWithGoogleDto = z.infer<typeof loginWithGoogleSchema>;
export type ResendEmailConfirmationDto = z.infer<typeof resendEmailConfirmationSchema>;
export type RefreshTokenDto = z.infer<typeof refreshTokenSchema>;
export type UserLoggedDto = z.infer<typeof userLoggedSchema>;
export type UserDto = z.infer<typeof userSchema>;
