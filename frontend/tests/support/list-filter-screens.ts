import type { ZodType } from 'zod';
import {
  adminUsersFilterFormSchema,
  adminUsersFilterToParams,
  adminUsersUserTypeFilterValues,
  defaultAdminUsersFilter
} from '@/features/admin/usuarios/list/admin-users-filter-schema';
import {
  defaultRecruitmentApplicationsFilter,
  recruitmentApplicationsFilterFormSchema,
  recruitmentApplicationsFilterToParams,
  recruitmentApplicationsStatusFilterValues
} from '@/features/recrutamento/candidaturas/recruitment-applications-filter-schema';
import {
  candidatesFilterSchema,
  candidatesFilterToParams,
  candidatesStatusFilterValues,
  defaultCandidatesFilter
} from '@/features/recrutamento/vagas/candidates/candidates-filter-schema';
import {
  defaultJobsFilter,
  jobsFilterFormSchema,
  jobsFilterToParams
} from '@/features/recrutamento/vagas/list/jobs-filter-schema';
import {
  hasActiveUrlSyncedParams,
  parseUrlSyncedParams,
  serializeUrlSyncedParams,
  type UrlSyncedValues
} from '@/shared/hooks/url-synced-params-codec';
import { expect } from 'chai';
import type { BusinessRulesWorld } from './world';

/**
 * Uma tela de listagem com filtro persistido na URL, reduzida ao que os cenários exercitam: os
 * defaults e o schema do formulário (entrada de `useUrlSyncedParams`), o codec da URL e o mapeamento
 * `*FilterToParams` que a página usa para montar a query da API.
 *
 * O tipo de cada formulário é apagado aqui (`UrlSyncedValues`) para que um único conjunto de steps
 * sirva às quatro telas; a tipagem forte continua nos módulos de produção, que são chamados sem cast
 * de valor além do necessário para voltar ao tipo do formulário.
 */
export type ListFilterScreen = {
  defaults: UrlSyncedValues;
  /** Valores aceitos pelos campos de enum do formulário (as opções que o select pode produzir). */
  enumValues: Readonly<Record<string, readonly string[]>>;
  isValid(values: UrlSyncedValues): boolean;
  parseUrl(query: string): UrlSyncedValues;
  serialize(values: UrlSyncedValues): URLSearchParams;
  hasActive(values: UrlSyncedValues): boolean;
  toParams(values: UrlSyncedValues): Record<string, unknown>;
};

function defineScreen<TValues extends UrlSyncedValues, TParams extends Record<string, unknown>>(config: {
  defaults: TValues;
  schema: ZodType<TValues>;
  toParams: (values: TValues) => TParams;
  enumValues: Readonly<Record<string, readonly string[]>>;
}): ListFilterScreen {
  const { defaults, schema, toParams, enumValues } = config;

  return {
    defaults,
    enumValues,
    isValid: (values) => schema.safeParse(values).success,
    parseUrl: (query) => parseUrlSyncedParams(new URLSearchParams(query), defaults, schema),
    serialize: (values) => serializeUrlSyncedParams(values as TValues, defaults),
    hasActive: (values) => hasActiveUrlSyncedParams(values as TValues, defaults),
    // Sem passar pelo schema: a página entrega a `*FilterToParams` os valores crus do formulário
    // (via `useFilterFormSync`), e é esse o caminho que o cenário precisa reproduzir.
    toParams: (values) => ({ ...toParams(values as TValues) })
  };
}

const SCREENS: Record<string, ListFilterScreen> = {
  'candidaturas do recrutamento': defineScreen({
    defaults: defaultRecruitmentApplicationsFilter,
    schema: recruitmentApplicationsFilterFormSchema,
    toParams: recruitmentApplicationsFilterToParams,
    enumValues: { status: recruitmentApplicationsStatusFilterValues }
  }),
  'candidatos da vaga': defineScreen({
    defaults: defaultCandidatesFilter,
    schema: candidatesFilterSchema,
    toParams: candidatesFilterToParams,
    enumValues: { status: candidatesStatusFilterValues }
  }),
  'vagas do recrutamento': defineScreen({
    defaults: defaultJobsFilter,
    schema: jobsFilterFormSchema,
    toParams: jobsFilterToParams,
    enumValues: {}
  }),
  'usuários do admin': defineScreen({
    defaults: defaultAdminUsersFilter,
    schema: adminUsersFilterFormSchema,
    toParams: adminUsersFilterToParams,
    enumValues: { userType: adminUsersUserTypeFilterValues }
  })
};

export function listFilterScreen(name: string): ListFilterScreen {
  const screen = SCREENS[name];
  expect(screen, `tela desconhecida: "${name}" (conhecidas: ${Object.keys(SCREENS).join(', ')})`).to.not.equal(
    undefined
  );
  return screen;
}

/** Valores que um campo de enum do formulário aceita — as únicas opções que o select pode produzir. */
export function enumValuesOf(tela: string, campo: string): readonly string[] {
  const values = listFilterScreen(tela).enumValues[campo];
  expect(values, `a tela "${tela}" não tem campo de enum "${campo}"`).to.not.equal(undefined);
  return values;
}

/** Estado de um cenário de filtro de listagem, guardado em `world.data`. */
export type ListFilterWorldData = {
  screen?: ListFilterScreen;
  query?: string;
  values?: UrlSyncedValues;
  serialized?: URLSearchParams;
  params?: Record<string, unknown>;
};

export function listFilterData(world: BusinessRulesWorld): ListFilterWorldData {
  return world.data as ListFilterWorldData;
}

export function currentScreen(world: BusinessRulesWorld): ListFilterScreen {
  const { screen } = listFilterData(world);
  expect(screen, 'nenhuma tela foi escolhida no cenário').to.not.equal(undefined);
  return screen!;
}

export function currentValues(world: BusinessRulesWorld): UrlSyncedValues {
  const { values } = listFilterData(world);
  expect(values, 'o filtro ainda não foi inicializado nem lido da URL').to.not.equal(undefined);
  return values!;
}

/** Lista separada por vírgula do Gherkin: `""` é lista vazia, não uma lista com um item vazio. */
export function parseGherkinList(raw: string): string[] {
  return raw
    .split(',')
    .map((item) => item.trim())
    .filter((item) => item.length > 0);
}
