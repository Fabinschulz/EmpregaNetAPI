import { axiosApi, createAxiosConfig } from '../axios';
import {
  changeMyPasswordFormSchema,
  confirmEmailResponseSchema,
  confirmEmailSchema,
  forgotPasswordResponseSchema,
  forgotPasswordSchema,
  loginSchema,
  loginWithGoogleSchema,
  profileFormSchema,
  refreshTokenSchema,
  registerSchema,
  resendEmailConfirmationSchema,
  resetPasswordFormSchema,
  resetPasswordResponseSchema,
  userLoggedSchema,
  userSchema,
  type ChangeMyPasswordFormValues,
  type ConfirmEmailDto,
  type ForgotPasswordDto,
  type LoginDto,
  type LoginWithGoogleDto,
  type ProfileFormValues,
  type RefreshTokenDto,
  type RegisterDto,
  type ResendEmailConfirmationDto,
  type ResetPasswordFormValues,
  type UserDto,
  type UserLoggedDto
} from './users-schema';

export async function register(dto: RegisterDto): Promise<string> {
  const body = registerSchema.parse(dto);
  const res = await axiosApi.post<string>('/api/users/register', body);
  return res.data;
}

export async function login(dto: LoginDto): Promise<UserLoggedDto> {
  const body = loginSchema.parse(dto);
  const res = await axiosApi.post<unknown>('/api/users/login', body);
  return userLoggedSchema.parse(res.data);
}

export async function loginWithGoogle(dto: LoginWithGoogleDto): Promise<UserLoggedDto> {
  const body = loginWithGoogleSchema.parse(dto);
  const res = await axiosApi.post<unknown>('/api/users/login/google', body);
  return userLoggedSchema.parse(res.data);
}

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

export async function forgotPassword(dto: ForgotPasswordDto): Promise<string> {
  const body = forgotPasswordSchema.parse(dto);
  const res = await axiosApi.post<unknown>('/api/users/forgot-password', body);
  const parsed = forgotPasswordResponseSchema.safeParse(res.data);
  if (parsed.success) return parsed.data.message;
  return 'Se o e-mail existir, enviámos instruções para redefinir a senha.';
}

export async function resetPassword(dto: ResetPasswordFormValues): Promise<string> {
  const body = resetPasswordFormSchema.parse(dto);
  const res = await axiosApi.post<unknown>('/api/users/reset-password', body);
  const parsed = resetPasswordResponseSchema.safeParse(res.data);
  if (parsed.success) return parsed.data.message;
  return 'Senha redefinida com sucesso.';
}

export async function confirmEmail(dto: ConfirmEmailDto): Promise<string> {
  const body = confirmEmailSchema.parse(dto);
  const res = await axiosApi.post<unknown>('/api/users/confirm-email', body);
  const parsed = confirmEmailResponseSchema.safeParse(res.data);
  if (parsed.success) return parsed.data.message;
  return 'E-mail confirmado com sucesso.';
}

export async function resendEmailConfirmation(dto: ResendEmailConfirmationDto): Promise<string> {
  const body = resendEmailConfirmationSchema.parse(dto);
  const res = await axiosApi.post<unknown>('/api/users/resend-email-confirmation', body);
  const parsed = forgotPasswordResponseSchema.safeParse(res.data);
  if (parsed.success) return parsed.data.message;
  return 'Se o e-mail existir e ainda não estiver confirmado, reenviámos o link.';
}

export async function me(): Promise<UserDto> {
  const res = await axiosApi.get<unknown>('/api/users/me', createAxiosConfig());
  return userSchema.parse(res.data);
}

/** Atualiza os dados da própria conta (username, e-mail, telefone). */
export async function updateMyProfile(dto: ProfileFormValues): Promise<UserDto> {
  const values = profileFormSchema.parse(dto);
  const body = {
    userName: values.username,
    email: values.email,
    phoneNumber: values.phoneNumber?.trim() || null
  };
  const res = await axiosApi.put<unknown>('/api/users/me', body, createAxiosConfig());
  return userSchema.parse(res.data);
}

/** Altera a própria senha (exige a senha atual). Revoga as sessões abertas no servidor. */
export async function changeMyPassword(dto: ChangeMyPasswordFormValues): Promise<string> {
  const body = changeMyPasswordFormSchema.parse(dto);
  const res = await axiosApi.post<unknown>('/api/users/me/change-password', body, createAxiosConfig());
  const parsed = confirmEmailResponseSchema.safeParse(res.data);
  if (parsed.success) return parsed.data.message;
  return 'Senha alterada com sucesso.';
}

/** Encerra a própria conta (exclusão lógica; o registro permanece para auditoria). */
export async function deleteMyAccount(): Promise<void> {
  await axiosApi.delete<unknown>('/api/users/me', createAxiosConfig());
}
