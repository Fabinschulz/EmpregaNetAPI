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

export type RefreshTokenDto = z.infer<typeof refreshTokenSchema>;
export type UserLoggedDto = z.infer<typeof userLoggedSchema>;
