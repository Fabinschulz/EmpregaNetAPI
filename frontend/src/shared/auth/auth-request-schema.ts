import { z } from 'zod';

export const refreshTokenRequestSchema = z.object({
  refreshToken: z.string().min(1, { message: 'O token de atualização é obrigatório.' })
});

export type RefreshTokenRequest = z.infer<typeof refreshTokenRequestSchema>;
