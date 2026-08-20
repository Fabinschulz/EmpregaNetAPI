export const publicJobsRoutes = {
  list: '/vagas',
  detail: (id: number) => `/vagas/${id}`
} as const;


export function publicJobUrl(id: number): string {
  return new URL(publicJobsRoutes.detail(id), window.location.origin).toString();
}
