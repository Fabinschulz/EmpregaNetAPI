import { z } from "zod";

export const registerSchema = z.object({
  username: z.string().min(3).max(64),
  email: z.string().email(),
  password: z.string().min(8),
  passwordConfirmation: z.string().min(8),
});

export const registerFormSchema = registerSchema.refine(
  (data) => data.password === data.passwordConfirmation,
  { message: "As senhas não conferem.", path: ["passwordConfirmation"] }
);

export const loginSchema = z.object({
  email: z.string().email(),
  password: z.string().min(1),
});

export const refreshTokenSchema = z.object({
  refreshToken: z.string().min(1),
});

export const forgotPasswordSchema = z.object({
  email: z.string().email(),
});

export const resetPasswordSchema = z.object({
  userId: z.string().min(1).optional(),
  token: z.string().min(1),
  password: z.string().min(8),
  passwordConfirmation: z.string().min(8),
  email: z.string().email().optional(),
});

export const confirmEmailSchema = z.object({
  userId: z.string().min(1),
  token: z.string().min(1),
});

export const userClaimSchema = z.object({
  value: z.string(),
  type: z.string(),
});

export const userTokenSchema = z.object({
  id: z.number().int(),
  username: z.string(),
  email: z.string().email(),
  roles: z.array(z.string()).default([]),
  claims: z.array(userClaimSchema),
});

export const userLoggedSchema = z.object({
  accessToken: z.string().min(1),
  expiresIn: z.number(),
  userToken: userTokenSchema,
  permissions: z.record(z.string(), z.array(z.string())).optional(),
  permissionCodes: z.array(z.string()).optional(),
  key: z.string().optional(),
  refreshToken: z.string().nullable().optional(),
});

export const userSchema = z.object({
  id: z.number().int(),
  username: z.string(),
  email: z.string().email(),
  phoneNumber: z.string().nullable().optional(),
  userType: z.string().nullable().optional(),
  isDeleted: z.boolean().optional(),
});

export type RegisterDto = z.infer<typeof registerSchema>;
export type LoginDto = z.infer<typeof loginSchema>;
export type ForgotPasswordDto = z.infer<typeof forgotPasswordSchema>;
export type ResetPasswordDto = z.infer<typeof resetPasswordSchema>;
export type ConfirmEmailDto = z.infer<typeof confirmEmailSchema>;
export type RefreshTokenDto = z.infer<typeof refreshTokenSchema>;
export type UserLoggedDto = z.infer<typeof userLoggedSchema>;
export type UserDto = z.infer<typeof userSchema>;
