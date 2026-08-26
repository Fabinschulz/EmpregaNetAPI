import { z } from 'zod';

export const userResponseSchema = z
  .object({
    id: z.number().int(),
    username: z.string().nullable().optional(),
    email: z.string().nullable().optional(),
    cpf: z.string().nullable().optional(),
    phoneNumber: z.string().nullable().optional(),
    userType: z.string().nullable().optional(),
    roles: z.array(z.string()).nullable().optional(),
    isDeleted: z.boolean().nullable().optional(),
    createdAt: z.string().nullable().optional(),
    updatedAt: z.string().nullable().optional(),
    deletedAt: z.string().nullable().optional()
  })
  .transform((user) => ({
    id: user.id,
    username: user.username ?? '',
    email: user.email ?? '',
    cpf: user.cpf,
    phoneNumber: user.phoneNumber ?? null,
    userType: user.userType ?? '',
    roles: user.roles ?? [],
    isDeleted: user.isDeleted ?? false,
    createdAt: user.createdAt ?? null,
    updatedAt: user.updatedAt ?? null,
    deletedAt: user.deletedAt ?? null
  }));

export type UserResponse = z.infer<typeof userResponseSchema>;
