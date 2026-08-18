export const adminUsersRoutes = {
  list: '/admin/usuarios',
  detail: (id: number) => `/admin/usuarios/${id}`
} as const;
