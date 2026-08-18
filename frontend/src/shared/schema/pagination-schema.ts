import { z } from 'zod';

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
