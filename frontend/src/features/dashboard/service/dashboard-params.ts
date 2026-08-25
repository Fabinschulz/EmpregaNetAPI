/**
 * Recortes do cabeçalho do dashboard, no formato da interface, e a tradução para a query da API.
 *
 * As datas vivem como `yyyy-MM-dd` — dia civil, sem hora e sem fuso. É deliberado: o servidor
 * interpreta esse dia no fuso de Brasília e devolve as fronteiras UTC que usou. Mandar um
 * `Date`/ISO daqui faria o navegador aplicar o fuso da máquina do utilizador e o período pedido
 * mudaria conforme quem abre a tela.
 */

/** Períodos oferecidos, na mesma ordem do seletor. Espelha `DashboardPeriodEnum` no servidor. */
export const DASHBOARD_PERIODS = ['Today', 'Last7Days', 'Last30Days', 'Last90Days', 'ThisYear', 'Custom'] as const;

export type DashboardPeriod = (typeof DASHBOARD_PERIODS)[number];

export const dashboardPeriodLabels: Record<DashboardPeriod, string> = {
  Today: 'Hoje',
  Last7Days: 'Últimos 7 dias',
  Last30Days: 'Últimos 30 dias',
  Last90Days: 'Últimos 90 dias',
  ThisYear: 'Este ano',
  Custom: 'Personalizado'
};

export const DASHBOARD_GRANULARITIES = ['Daily', 'Weekly', 'Monthly'] as const;

export type DashboardGranularity = (typeof DASHBOARD_GRANULARITIES)[number];

export const dashboardGranularityLabels: Record<DashboardGranularity, string> = {
  Daily: 'Diário',
  Weekly: 'Semanal',
  Monthly: 'Mensal'
};

export const DASHBOARD_JOB_RANKINGS = ['MostApplications', 'FewestApplications', 'MostRecent'] as const;

export type DashboardJobRanking = (typeof DASHBOARD_JOB_RANKINGS)[number];

export const dashboardJobRankingLabels: Record<DashboardJobRanking, string> = {
  MostApplications: 'Mais candidaturas',
  FewestApplications: 'Menos candidaturas',
  MostRecent: 'Mais recentes'
};

/** Estado dos filtros, como a interface o mantém. */
export type DashboardFilters = {
  period: DashboardPeriod;
  /** `yyyy-MM-dd`; obrigatório quando `period` é `Custom`. */
  from?: string;
  to?: string;
  companyId?: number;
  /** Siglas de UF (`SP`, `MG`). */
  states?: string[];
  /** Nomes de área como o servidor os declara (`Producao`, `Ti`). */
  areas?: string[];
  /** Nome do status da candidatura (`Approved`). */
  applicationStatus?: string;
};

export const defaultDashboardFilters: DashboardFilters = {
  period: 'Last30Days'
};

/**
 * Parâmetros aceitos pelos endpoints do dashboard.
 *
 * Chaves no singular para os filtros de múltipla escolha (`state`, `area`): o cliente HTTP
 * serializa arrays repetindo a chave, mesma convenção do feed de vagas.
 */
export type DashboardQueryParams = {
  period: DashboardPeriod;
  from?: string;
  to?: string;
  companyId?: number;
  state?: string[];
  area?: string[];
  status?: string;
};

/**
 * Traduz os filtros da interface para a query da API, omitindo o que não recorta nada.
 *
 * Coleções vazias e strings vazias são removidas em vez de enviadas: `?state=` chegaria ao servidor
 * como uma UF não reconhecida, e o painel voltaria vazio sem que nada na tela explicasse por quê.
 */
export function dashboardFiltersToParams(filters: DashboardFilters): DashboardQueryParams {
  const isCustom = filters.period === 'Custom';

  return {
    period: filters.period,
    from: isCustom ? filters.from : undefined,
    to: isCustom ? filters.to : undefined,
    companyId: filters.companyId,
    state: filters.states?.length ? filters.states : undefined,
    area: filters.areas?.length ? filters.areas : undefined,
    status: filters.applicationStatus || undefined
  };
}

/**
 * Chave estável dos filtros, para o cache do React Query.
 *
 * Serializa em ordem fixa e com as coleções ordenadas: sem isso, `['SP','MG']` e `['MG','SP']`
 * gerariam duas entradas de cache para exatamente a mesma pergunta.
 */
export function dashboardFiltersToKey(filters: DashboardFilters): string {
  const params = dashboardFiltersToParams(filters);

  return JSON.stringify({
    period: params.period,
    from: params.from ?? null,
    to: params.to ?? null,
    companyId: params.companyId ?? null,
    state: params.state ? [...params.state].sort() : null,
    area: params.area ? [...params.area].sort() : null,
    status: params.status ?? null
  });
}
