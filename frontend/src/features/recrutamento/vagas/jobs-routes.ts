/** Rotas da entidade Vaga, em um lugar só. Ver `companiesRoutes` para o porquê. */
export const jobsRoutes = {
  list: '/recrutamento/vagas',
  new: '/recrutamento/vagas/new',
  detail: (id: number) => `/recrutamento/vagas/${id}`,
  candidates: (id: number) => `/recrutamento/vagas/${id}/candidatos`
} as const;
