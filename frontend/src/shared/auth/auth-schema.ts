import { z } from 'zod';

export const refreshTokenSchema = z.object({
  refreshToken: z.string().min(1, { message: 'O token de atualização é obrigatório.' })
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
  isDeleted: z.boolean().optional(),
  createdAt: z.string().nullable().optional(),
  updatedAt: z.string().nullable().optional(),
  deletedAt: z.string().nullable().optional()
});

export type RefreshTokenDto = z.infer<typeof refreshTokenSchema>;
export type UserLoggedDto = z.infer<typeof userLoggedSchema>;
export type UserDto = z.infer<typeof userSchema>;
