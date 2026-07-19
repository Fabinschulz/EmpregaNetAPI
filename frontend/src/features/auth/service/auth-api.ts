import { axiosApi, userLoggedSchema, type UserLoggedDto } from '@/shared';
import {
    authMessageResponseSchema,
    confirmEmailSchema,
    forgotPasswordSchema,
    loginSchema,
    loginWithGoogleSchema,
    registerSchema,
    resendEmailConfirmationSchema,
    resetPasswordFormSchema,
    type ConfirmEmailDto,
    type ForgotPasswordDto,
    type LoginDto,
    type LoginWithGoogleDto,
    type RegisterDto,
    type ResendEmailConfirmationDto,
    type ResetPasswordFormValues
} from './auth-schema';

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

export async function forgotPassword(dto: ForgotPasswordDto): Promise<string> {
  const body = forgotPasswordSchema.parse(dto);
  const res = await axiosApi.post<unknown>('/api/users/forgot-password', body);
  const parsed = authMessageResponseSchema.safeParse(res.data);
  if (parsed.success) return parsed.data.message;
  return 'Se o e-mail existir, enviámos instruções para redefinir a senha.';
}

export async function resetPassword(dto: ResetPasswordFormValues): Promise<string> {
  const body = resetPasswordFormSchema.parse(dto);
  const res = await axiosApi.post<unknown>('/api/users/reset-password', body);
  const parsed = authMessageResponseSchema.safeParse(res.data);
  if (parsed.success) return parsed.data.message;
  return 'Senha redefinida com sucesso.';
}

export async function confirmEmail(dto: ConfirmEmailDto): Promise<string> {
  const body = confirmEmailSchema.parse(dto);
  const res = await axiosApi.post<unknown>('/api/users/confirm-email', body);
  const parsed = authMessageResponseSchema.safeParse(res.data);
  if (parsed.success) return parsed.data.message;
  return 'E-mail confirmado com sucesso.';
}

export async function resendEmailConfirmation(dto: ResendEmailConfirmationDto): Promise<string> {
  const body = resendEmailConfirmationSchema.parse(dto);
  const res = await axiosApi.post<unknown>('/api/users/resend-email-confirmation', body);
  const parsed = authMessageResponseSchema.safeParse(res.data);
  if (parsed.success) return parsed.data.message;
  return 'Se o e-mail existir e ainda não estiver confirmado, reenviámos o link.';
}
