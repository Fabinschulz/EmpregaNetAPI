import { loginSchema, type LoginDto } from '../service';

export { loginSchema, type LoginDto };

export const loginDefaultValues: LoginDto = {
  login: '',
  password: ''
};
