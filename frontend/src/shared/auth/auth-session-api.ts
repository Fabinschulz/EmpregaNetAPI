import { axiosApi } from '@/shared';
import { refreshTokenSchema, userLoggedSchema, type RefreshTokenDto, type UserLoggedDto } from './auth-schema';

/**
 * Renova a sessão. Sem argumento, o refresh token é lido do cookie httpOnly
 * (enviado automaticamente por `withCredentials`); `dto` só é usado em casos legados/testes.
 */
export async function refreshToken(dto?: RefreshTokenDto): Promise<UserLoggedDto> {
  const body = dto ? refreshTokenSchema.parse(dto) : undefined;
  const res = await axiosApi.post<unknown>('/api/users/refresh-token', body);
  return userLoggedSchema.parse(res.data);
}

/** Encerra a sessão no servidor (revoga o refresh token do cookie httpOnly e limpa os cookies de auth). */
export async function logout(): Promise<void> {
  await axiosApi.post<unknown>('/api/users/logout', undefined);
}
