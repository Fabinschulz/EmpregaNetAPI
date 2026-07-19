import { registerSchema } from '@/features/auth/service';
import { isValidBrazilCellPhone, maskBrazilPhone } from '@/shared';
import { z } from 'zod';

export const profileFormSchema = registerSchema.pick({ username: true, email: true }).extend({
  phoneNumber: z
    .string()
    .optional()
    .nullable()
    .refine((value) => !value || value.trim() === '' || isValidBrazilCellPhone(value), {
      message: 'Celular inválido. Informe DDD + 9 dígitos, ex.: (11) 98765-4321.'
    })
});

export type ProfileFormValues = z.infer<typeof profileFormSchema>;

export const defaultProfileForm: ProfileFormValues = {
  username: '',
  email: '',
  phoneNumber: ''
};

export function profileFormValuesFromDto(user: {
  username: string;
  email: string;
  phoneNumber?: string | null;
}): ProfileFormValues {
  return {
    username: user.username,
    email: user.email,
    phoneNumber: user.phoneNumber ? maskBrazilPhone(user.phoneNumber) : ''
  };
}

export const changeMyPasswordFormSchema = z
  .object({
    currentPassword: z.string().min(1, { message: 'Informe a senha atual.' }),
    newPassword: z.string().min(8, { message: 'A nova senha deve ter pelo menos 8 caracteres.' }),
    newPasswordConfirmation: z.string().min(8, { message: 'Confirme a nova senha.' })
  })
  .refine((data) => data.newPassword === data.newPasswordConfirmation, {
    message: 'As senhas não conferem.',
    path: ['newPasswordConfirmation']
  });

export type ChangeMyPasswordFormValues = z.infer<typeof changeMyPasswordFormSchema>;

export const defaultChangeMyPasswordForm: ChangeMyPasswordFormValues = {
  currentPassword: '',
  newPassword: '',
  newPasswordConfirmation: ''
};
