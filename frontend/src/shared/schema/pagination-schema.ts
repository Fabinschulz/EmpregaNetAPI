import { z } from 'zod';

/**
 * Parâmetros de paginação/ordenação enviados à API, alinhados a `ListQueryParams` do backend
 * (`page`, `size`, `orderBy`). Schema Zod validável (com defaults), em vez de um tipo TS solto —
 * permite validar entradas vindas de UI (ex.: um seletor de itens por página) antes de
 * repassar à camada de serviços.
 */
export const paginationParamsSchema = z.object({
  page: z.number().int().positive({ message: 'A página deve ser maior ou igual a 1.' }).default(1),
  size: z
    .number()
    .int()
    .positive({ message: 'O tamanho de página deve ser maior que 0.' })
    .max(200, { message: 'O tamanho de página não pode exceder 200.' })
    .default(20),
  orderBy: z.string().trim().min(1).optional()
});

export type PaginationParams = z.infer<typeof paginationParamsSchema>;

/** Tamanhos de página padrão oferecidos nas listagens (seletor "itens por página"). */
export const PAGE_SIZE_OPTIONS = [10, 20, 50, 100] as const;

export const DEFAULT_PAGE = 1;
export const DEFAULT_PAGE_SIZE: (typeof PAGE_SIZE_OPTIONS)[number] = 20;

/**
 * Metadados de paginação devolvidos pela API junto com os itens da página atual.
 * Alinhado ao `ListDataPagination<T>` do backend: `page`, `totalPages`, `totalItems`.
 */
export const paginationMetaSchema = z.object({
  page: z.number().int().nonnegative().optional(),
  totalPages: z.number().int().nonnegative().optional(),
  totalItems: z.number().int().nonnegative().optional()
});

export type PaginationMetaResponse = z.infer<typeof paginationMetaSchema>;

/**
 * Cria o schema de uma resposta paginada da API para o schema de item `T` informado.
 *
 * Uso: `const jobsListResponseSchema = createPaginatedResponseSchema(jobSchema);`
 */
export function createPaginatedResponseSchema<TItem extends z.ZodTypeAny>(itemSchema: TItem) {
  return paginationMetaSchema.extend({
    data: z.array(itemSchema)
  });
}

export type PaginatedResponse<TItem> = PaginationMetaResponse & { data: TItem[] };

/**
 * Calcula o total de páginas a partir do total de itens e do tamanho de página.
 * Devolve sempre pelo menos 1 (mesmo sem itens), para uma UI de paginação nunca ficar "0 de 0".
 */
export function computeTotalPages(totalItems: number | undefined, pageSize: number): number {
  if (!totalItems || totalItems <= 0 || pageSize <= 0) return 1;
  return Math.max(1, Math.ceil(totalItems / pageSize));
}

/** Garante que a página informada esteja dentro do intervalo válido `[1, totalPages]`. */
export function clampPage(page: number, totalPages: number): number {
  if (page < 1) return 1;
  if (page > totalPages) return totalPages;
  return page;
}
