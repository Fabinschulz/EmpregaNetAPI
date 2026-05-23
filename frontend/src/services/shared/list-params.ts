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

export type JobsListQueryParams = ListQueryParams & Pick<SoftDeleteFilterParams, 'isActive'>;

export type CompaniesListQueryParams = ListQueryParams & SoftDeleteFilterParams;

export type AdminUsersListQueryParams = ListQueryParams & Pick<SoftDeleteFilterParams, 'isDeleted'>;

export type CandidatesListQueryParams = ListQueryParams;

export type JobApplicationsListQueryParams = ListQueryParams & StatusFilterParams;

export type JobApplicationsAdminListQueryParams = ListQueryParams & SoftDeleteFilterParams;

export const DEFAULT_LIST_PAGE = 1;
export const DEFAULT_LIST_SIZE = 100;

export function withDefaultListParams<T extends ListQueryParams>(params?: T): T {
  return { page: DEFAULT_LIST_PAGE, size: DEFAULT_LIST_SIZE, ...params } as T;
}
