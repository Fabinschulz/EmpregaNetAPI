export const companiesRoutes = {
  list: '/admin/empresas',
  new: '/admin/empresas/new',
  detail: (id: number) => `/admin/empresas/${id}`
} as const;
