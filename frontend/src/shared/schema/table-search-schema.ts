import { z } from 'zod';
import { paginationParamsSchema } from './pagination-schema';

/**
 * Termo de busca livre (ex.: campo "Buscar por título"), normalizado e limitado em tamanho.
 * String vazia/só espaços vira `undefined` — evita enviar `search=""` à API.
 */
export const searchTermSchema = z
  .string()
  .trim()
  .max(120, { message: 'A busca não pode exceder 120 caracteres.' })
  .optional()
  .transform((value) => (value && value.length > 0 ? value : undefined));

export const sortDirectionSchema = z.enum(['asc', 'desc']);
export type SortDirection = z.infer<typeof sortDirectionSchema>;

/**
 * Compõe os filtros de busca de uma tabela (paginação + busca livre + filtros específicos
 * da feature) num único schema Zod, reutilizável por qualquer tela de listagem do sistema.
 *
 * Uso: `const jobsTableSearchSchema = createTableSearchSchema({ isActive: z.boolean().optional() });`
 */
export function createTableSearchSchema<TFilters extends z.ZodRawShape>(filters: TFilters) {
  return paginationParamsSchema.extend({
    search: searchTermSchema,
    ...filters
  });
}

export type TableSearchErrors = Partial<Record<string, string>>;

/**
 * Converte um `ZodError` de filtros de tabela num mapa `{ campo: mensagem }`, no mesmo
 * espírito dos erros exibidos pelo `FormProvider` — pronto para renderizar ao lado de
 * cada campo de filtro.
 */
export function extractTableSearchErrors(error: z.ZodError): TableSearchErrors {
  const errors: TableSearchErrors = {};
  for (const issue of error.issues) {
    const key = issue.path.length > 0 ? String(issue.path[0]) : '_root';
    if (!errors[key]) errors[key] = issue.message;
  }
  return errors;
}

export type TableSearchValidationResult<TValues> =
  | { success: true; data: TValues }
  | { success: false; errors: TableSearchErrors };

/**
 * Valida os filtros de uma tabela sem lançar exceção; devolve os valores normalizados
 * ou um mapa de erros por campo. Pensado para uso direto em handlers de "aplicar filtro".
 */
export function validateTableSearch<TSchema extends z.ZodTypeAny>(
  schema: TSchema,
  values: unknown
): TableSearchValidationResult<z.infer<TSchema>> {
  const result = schema.safeParse(values);
  if (result.success) return { success: true, data: result.data };
  return { success: false, errors: extractTableSearchErrors(result.error) };
}
