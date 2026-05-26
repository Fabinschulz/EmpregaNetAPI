import { resendEmailConfirmationSchema, type ResendEmailConfirmationDto } from '@/services';

export { resendEmailConfirmationSchema, type ResendEmailConfirmationDto };

export const resendConfirmationDefaultValues: ResendEmailConfirmationDto = {
  email: ''
};
