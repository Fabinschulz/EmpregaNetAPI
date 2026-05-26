import { forgotPasswordSchema, type ForgotPasswordDto } from '@/services';

export { forgotPasswordSchema, type ForgotPasswordDto };

export const forgotPasswordDefaultValues: ForgotPasswordDto = {
  email: ''
};
