import { registerFormSchema, type RegisterDto } from '../service';

export { registerFormSchema, type RegisterDto };

export const registerDefaultValues: RegisterDto = {
  username: '',
  email: '',
  password: '',
  passwordConfirmation: ''
};
