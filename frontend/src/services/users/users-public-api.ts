import { axiosApi, createAxiosConfig } from "../axios-api";
import {
  confirmEmailSchema,
  forgotPasswordSchema,
  loginSchema,
  refreshTokenSchema,
  registerSchema,
  resetPasswordSchema,
  userLoggedSchema,
  userSchema,
  type ConfirmEmailDto,
  type ForgotPasswordDto,
  type LoginDto,
  type RefreshTokenDto,
  type RegisterDto,
  type ResetPasswordDto,
  type UserDto,
  type UserLoggedDto,
} from "./users-schema";

export async function register(dto: RegisterDto): Promise<string> {
  const body = registerSchema.parse(dto);
  const res = await axiosApi.post<string>("/api/users/register", body);
  return res.data;
}

export async function login(dto: LoginDto): Promise<UserLoggedDto> {
  const body = loginSchema.parse(dto);
  const res = await axiosApi.post<unknown>("/api/users/login", body);
  return userLoggedSchema.parse(res.data);
}

export async function refreshToken(dto: RefreshTokenDto): Promise<UserLoggedDto> {
  const body = refreshTokenSchema.parse(dto);
  const res = await axiosApi.post<unknown>("/api/users/refresh-token", body);
  return userLoggedSchema.parse(res.data);
}

export async function forgotPassword(dto: ForgotPasswordDto): Promise<unknown> {
  const body = forgotPasswordSchema.parse(dto);
  const res = await axiosApi.post<unknown>("/api/users/forgot-password", body);
  return res.data;
}

export async function resetPassword(dto: ResetPasswordDto): Promise<unknown> {
  const body = resetPasswordSchema.parse(dto);
  const res = await axiosApi.post<unknown>("/api/users/reset-password", body);
  return res.data;
}

export async function confirmEmail(dto: ConfirmEmailDto): Promise<unknown> {
  const body = confirmEmailSchema.parse(dto);
  const res = await axiosApi.post<unknown>("/api/users/confirm-email", body);
  return res.data;
}

export async function resendEmailConfirmation(email: string): Promise<unknown> {
  const res = await axiosApi.post<unknown>("/api/users/resend-email-confirmation", { email });
  return res.data;
}

export async function me(token: string): Promise<UserDto> {
  const res = await axiosApi.get<unknown>("/api/users/me", createAxiosConfig(token));
  return userSchema.parse(res.data);
}
