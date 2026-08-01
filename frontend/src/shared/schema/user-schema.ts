import { z } from 'zod';

export const userSchema = z.object({
  id: z.number().int(),
  username: z
    .string()
    .min(3, { message: 'O nome de usuário deve ter pelo menos 3 caracteres.' })
    .max(64, { message: 'O nome de usuário deve ter no máximo 64 caracteres.' }),
  email: z.string().email({ message: 'E-mail inválido.' }),
  phoneNumber: z.string().nullable().optional(),
  userType: z.string().nullable().optional(),
  roles: z.array(z.string()).default([]),
  isDeleted: z.boolean().optional(),
  createdAt: z.string().nullable().optional(),
  updatedAt: z.string().nullable().optional(),
  deletedAt: z.string().nullable().optional()
});

export type UserDto = z.infer<typeof userSchema>;
