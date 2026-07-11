import { DEFAULT_PAGE } from './pagination-schema';

/** Paginação e ordenação comuns a endpoints de listagem. */
export type ListQueryParams = {
  page?: number;
  size?: number;
  orderBy?: string;
};

/** Filtros de exclusão lógica / ativo usados em listagens administrativas. */
export type SoftDeleteFilterParams = {
  isDeleted?: boolean;
  isActive?: boolean;
};

export type StatusFilterParams = {
  status?: string;
};

/** Busca textual livre (interpretação por entidade no backend; ex.: título/descrição em vagas). */
export type SearchFilterParams = {
  search?: string;
};

export type JobsListQueryParams = ListQueryParams & Pick<SoftDeleteFilterParams, 'isActive'> & SearchFilterParams;

export type CompaniesListQueryParams = ListQueryParams & SoftDeleteFilterParams;

export type AdminUsersListQueryParams = ListQueryParams & Pick<SoftDeleteFilterParams, 'isDeleted'>;

export type CandidatesListQueryParams = ListQueryParams;

export type JobApplicationsListQueryParams = ListQueryParams & StatusFilterParams;

export type JobApplicationsAdminListQueryParams = ListQueryParams & SoftDeleteFilterParams;

/**
 * Tamanho usado pelas telas ainda NÃO migradas para tabela paginada ("traga tudo").
 * Transitório: quando uma tela adota `usePersistedTablePagination`, ela passa `page`/`size`
 * explícitos e deixa de depender deste default. Remover quando todas migrarem.
 */
export const FULL_LIST_SIZE = 100;

export function withDefaultListParams<T extends ListQueryParams>(params?: T): T {
  return { page: DEFAULT_PAGE, size: FULL_LIST_SIZE, ...params } as T;
}
