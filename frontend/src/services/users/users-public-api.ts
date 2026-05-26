import { axiosApi, createAxiosConfig } from '../axios';
import {
  confirmEmailResponseSchema,
  confirmEmailSchema,
  forgotPasswordResponseSchema,
  forgotPasswordSchema,
  loginSchema,
  loginWithGoogleSchema,
  refreshTokenSchema,
  registerSchema,
  resendEmailConfirmationSchema,
  resetPasswordFormSchema,
  resetPasswordResponseSchema,
  userLoggedSchema,
  userSchema,
  type ConfirmEmailDto,
  type ForgotPasswordDto,
  type LoginDto,
  type LoginWithGoogleDto,
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

export async function refreshToken(dto: RefreshTokenDto): Promise<UserLoggedDto> {
  const body = refreshTokenSchema.parse(dto);
  const res = await axiosApi.post<unknown>('/api/users/refresh-token', body);
  return userLoggedSchema.parse(res.data);
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

export async function me(token: string): Promise<UserDto> {
  const res = await axiosApi.get<unknown>('/api/users/me', createAxiosConfig(token));
  return userSchema.parse(res.data);
}
