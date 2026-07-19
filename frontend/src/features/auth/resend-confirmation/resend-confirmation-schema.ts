import { resendEmailConfirmationSchema, type ResendEmailConfirmationDto } from '../service';

export { resendEmailConfirmationSchema, type ResendEmailConfirmationDto };

export const resendConfirmationDefaultValues: ResendEmailConfirmationDto = {
  email: ''
};
