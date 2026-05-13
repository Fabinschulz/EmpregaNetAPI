import { loginSchema, type LoginDto } from '@/services';

export { loginSchema, type LoginDto };

export const loginDefaultValues: LoginDto = {
  email: '',
  password: ''
};
