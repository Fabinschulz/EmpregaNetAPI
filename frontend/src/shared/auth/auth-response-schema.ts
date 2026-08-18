import { z } from 'zod';

/**
 * O que os endpoints de sessão devolvem (`UserLoggedViewModel`).
 *
 * `permissions` e `permissionCodes` são propriedades **calculadas** no servidor e sempre
 * serializadas (dicionário/array vazios quando não há permissão), por isso não são opcionais aqui.
 * `key` idem, com string vazia por padrão. `refreshToken` é o único de fato anulável: nesta
 * aplicação ele viaja em cookie httpOnly e o corpo costuma trazer `null`.
 *
 * O campo `Permissions` (lista interna de enums) tem `[JsonIgnore]` e não chega ao cliente — o que
 * chega é a projeção `permissions` agrupada por recurso.
 */

const userClaimResponseSchema = z.object({
  value: z.string(),
  type: z.string()
});

const userTokenResponseSchema = z.object({
  id: z.number().int(),
  username: z.string(),
  email: z.string(),
  roles: z.array(z.string()),
  claims: z.array(userClaimResponseSchema)
});

export const userLoggedResponseSchema = z.object({
  accessToken: z.string(),
  expiresIn: z.number(),
  userToken: userTokenResponseSchema,
  permissions: z.record(z.string(), z.array(z.string())),
  permissionCodes: z.array(z.string()),
  key: z.string(),
  refreshToken: z.string().nullable().optional()
});

export type UserLoggedResponse = z.infer<typeof userLoggedResponseSchema>;
