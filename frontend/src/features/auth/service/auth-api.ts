import { axiosApi } from '@/shared/api';
import { readMessageOr } from '@/shared/schema';
import { userLoggedResponseSchema, type UserLoggedResponse } from '@/shared/auth';
import {
  confirmEmailRequestSchema,
  forgotPasswordRequestSchema,
  loginRequestSchema,
  loginWithGoogleRequestSchema,
  registerRequestSchema,
  resendEmailConfirmationRequestSchema,
  resetPasswordRequestSchema,
  type ConfirmEmailRequest,
  type ForgotPasswordRequest,
  type LoginRequest,
  type LoginWithGoogleRequest,
  type RegisterRequest,
  type ResendEmailConfirmationRequest,
  type ResetPasswordRequest
} from './auth-request-schema';

export async function register(request: RegisterRequest): Promise<string> {
  const body = registerRequestSchema.parse(request);
  const res = await axiosApi.post<string>('/api/auth/register', body);
  return res.data;
}

export async function login(request: LoginRequest): Promise<UserLoggedResponse> {
  const body = loginRequestSchema.parse(request);
  const res = await axiosApi.post<unknown>('/api/auth/login', body);
  return userLoggedResponseSchema.parse(res.data);
}

export async function loginWithGoogle(request: LoginWithGoogleRequest): Promise<UserLoggedResponse> {
  const body = loginWithGoogleRequestSchema.parse(request);
  const res = await axiosApi.post<unknown>('/api/auth/login/google', body);
  return userLoggedResponseSchema.parse(res.data);
}

export async function forgotPassword(request: ForgotPasswordRequest): Promise<string> {
  const body = forgotPasswordRequestSchema.parse(request);
  const res = await axiosApi.post<unknown>('/api/auth/forgot-password', body);
  return readMessageOr(res.data, 'Se o e-mail existir, enviámos instruções para redefinir a senha.');
}

export async function resetPassword(request: ResetPasswordRequest): Promise<string> {
  const body = resetPasswordRequestSchema.parse(request);
  const res = await axiosApi.post<unknown>('/api/auth/reset-password', body);
  return readMessageOr(res.data, 'Senha redefinida com sucesso.');
}

export async function confirmEmail(request: ConfirmEmailRequest): Promise<string> {
  const body = confirmEmailRequestSchema.parse(request);
  const res = await axiosApi.post<unknown>('/api/auth/confirm-email', body);
  return readMessageOr(res.data, 'E-mail confirmado com sucesso.');
}

export async function resendEmailConfirmation(request: ResendEmailConfirmationRequest): Promise<string> {
  const body = resendEmailConfirmationRequestSchema.parse(request);
  const res = await axiosApi.post<unknown>('/api/auth/resend-email-confirmation', body);
  return readMessageOr(res.data, 'Se o e-mail existir e ainda não estiver confirmado, reenviámos o link.');
}
