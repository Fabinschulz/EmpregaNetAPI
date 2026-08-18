import { axiosBase } from '@/shared/api';
import { refreshTokenRequestSchema, type RefreshTokenRequest } from './auth-request-schema';
import { userLoggedResponseSchema, type UserLoggedResponse } from './auth-response-schema';

/**
 * Renova a sessão. Sem argumento, o refresh token é lido do cookie httpOnly
 * (enviado automaticamente por `withCredentials`).
 */
export async function refreshToken(request?: RefreshTokenRequest): Promise<UserLoggedResponse> {
  const body = request ? refreshTokenRequestSchema.parse(request) : undefined;
  const res = await axiosBase.post<unknown>('/api/auth/refresh-token', body);
  return userLoggedResponseSchema.parse(res.data);
}

/** Encerra a sessão no servidor (revoga o refresh token do cookie httpOnly e limpa os cookies de auth). */
export async function logout(): Promise<void> {
  await axiosBase.post<unknown>('/api/auth/logout', undefined);
}
