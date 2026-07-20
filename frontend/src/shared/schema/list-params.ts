import { DEFAULT_PAGE } from './pagination-schema';

/** Paginação e ordenação comuns a endpoints de listagem. */
export type ListQueryParams = {
  page?: number;
  size?: number;
  orderBy?: string;
};

/** Valores de ordenação aceitos pelos endpoints de listagem (data de criação e id). */
export const LIST_ORDER_BY_VALUES = ['createdAt_DESC', 'createdAt_ASC', 'id_DESC', 'id_ASC'] as const;

export type ListOrderByValue = (typeof LIST_ORDER_BY_VALUES)[number];

export const LIST_ORDER_BY_OPTIONS: ReadonlyArray<{ value: ListOrderByValue; label: string }> = [
  { value: 'createdAt_DESC', label: 'Mais recentes' },
  { value: 'createdAt_ASC', label: 'Mais antigas' }
  // { value: 'id_DESC', label: 'Usuários mais recentes' },
  // { value: 'id_ASC', label: 'Usuários mais antigos' }
];

export const DATE_ORDER_BY_OPTIONS: ReadonlyArray<{ value: ListOrderByValue; label: string }> = [
  { value: 'createdAt_DESC', label: 'Mais recentes' },
  { value: 'createdAt_ASC', label: 'Mais antigas' }
];

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

export type CompaniesListQueryParams = ListQueryParams & SoftDeleteFilterParams & SearchFilterParams;

export type AdminUsersListQueryParams = ListQueryParams &
  Pick<SoftDeleteFilterParams, 'isDeleted'> &
  SearchFilterParams;

export type CandidatesListQueryParams = ListQueryParams & SearchFilterParams;

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
