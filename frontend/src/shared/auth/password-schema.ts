import { z } from 'zod';

export const PASSWORD_MIN_LENGTH = 8;
export const NEW_PASSWORD_RULE_HINT =
  'Mínimo de 8 caracteres, com maiúscula, minúscula, dígito e um caractere especial (@$!%*?&).';

export const newPasswordSchema = z
  .string()
  .min(PASSWORD_MIN_LENGTH, { message: `A senha deve ter pelo menos ${PASSWORD_MIN_LENGTH} caracteres.` })
  .regex(/^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[@$!%*?&])[A-Za-z\d@$!%*?&]{8,}$/, { message: NEW_PASSWORD_RULE_HINT });

export const existingPasswordSchema = z.string().min(1, { message: 'A senha é obrigatória.' });
export const passwordConfirmationSchema = z.string().min(1, { message: 'Confirme a senha.' });
