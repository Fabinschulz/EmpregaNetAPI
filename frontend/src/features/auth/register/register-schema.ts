import { registerFormSchema, type RegisterDto } from "@/services";

export { registerFormSchema, type RegisterDto };

export const registerDefaultValues: RegisterDto = {
  username: "",
  email: "",
  password: "",
  passwordConfirmation: "",
};
