import { resetPasswordFormSchema, type ResetPasswordFormValues } from '@/services';

export { resetPasswordFormSchema, type ResetPasswordFormValues };

export function resetPasswordDefaultValues(userId: number, token: string): ResetPasswordFormValues {
  return {
    userId,
    token,
    newPassword: '',
    newPasswordConfirmation: ''
  };
}
