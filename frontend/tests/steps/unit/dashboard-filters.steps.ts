import { Given, Then, When } from '@cucumber/cucumber';
import { expect } from 'chai';
import {
  dashboardFilterFormToFilters,
  defaultDashboardFilterForm,
  type DashboardFilterFormValues
} from '@/features/dashboard/analytics/filters/dashboard-filter-schema';
import {
  dashboardFiltersToKey,
  dashboardFiltersToParams,
  type DashboardFilters,
  type DashboardQueryParams
} from '@/features/dashboard/service/dashboard-params';
import type { BusinessRulesWorld } from '../../support/world';

type DashboardWorldData = {
  form?: DashboardFilterFormValues;
  filters?: DashboardFilters | null;
  params?: DashboardQueryParams;
  savedKey?: string;
};

function dashboardData(world: BusinessRulesWorld): DashboardWorldData {
  return world.data as DashboardWorldData;
}

function currentForm(world: BusinessRulesWorld): DashboardFilterFormValues {
  const { form } = dashboardData(world);
  expect(form, 'o formulário ainda não foi inicializado').to.not.equal(undefined);
  return form!;
}

function currentFilters(world: BusinessRulesWorld): DashboardFilters {
  const { filters } = dashboardData(world);
  expect(filters, 'o recorte não foi produzido').to.not.equal(null);
  expect(filters, 'o recorte ainda não foi convertido').to.not.equal(undefined);
  return filters!;
}

function currentParams(world: BusinessRulesWorld): DashboardQueryParams {
  const { params } = dashboardData(world);
  expect(params, 'os parâmetros ainda não foram derivados').to.not.equal(undefined);
  return params!;
}

/** Lista da tabela do cenário: `""` significa lista vazia, não uma lista com um item vazio. */
function parseList(raw: string): string[] {
  return raw
    .split(',')
    .map((item) => item.trim())
    .filter((item) => item.length > 0);
}

Given('que o formulário de filtros do dashboard está com os valores padrão', function (this: BusinessRulesWorld) {
  dashboardData(this).form = { ...defaultDashboardFilterForm };
});

Given(
  'que o campo {string} do formulário é {string}',
  function (this: BusinessRulesWorld, field: string, value: string) {
    const form = currentForm(this) as unknown as Record<string, unknown>;
    form[field] = value;
  }
);

Given(
  'que o campo {string} do formulário é a lista {string}',
  function (this: BusinessRulesWorld, field: string, value: string) {
    const form = currentForm(this) as unknown as Record<string, unknown>;
    form[field] = parseList(value);
  }
);

When('eu converto o formulário em recorte', function (this: BusinessRulesWorld) {
  const filters = dashboardFilterFormToFilters(currentForm(this));
  dashboardData(this).filters = filters;
  dashboardData(this).params = filters ? dashboardFiltersToParams(filters) : undefined;
});

When('eu guardo a chave de cache do recorte', function (this: BusinessRulesWorld) {
  dashboardData(this).savedKey = dashboardFiltersToKey(currentFilters(this));
});

Then('o recorte deve existir', function (this: BusinessRulesWorld) {
  expect(dashboardData(this).filters, 'esperava um recorte completo').to.not.equal(null);
});

Then('o recorte não deve existir', function (this: BusinessRulesWorld) {
  expect(dashboardData(this).filters, 'esperava recorte incompleto (null)').to.equal(null);
});

Then('o parâmetro {string} deve ser {string}', function (this: BusinessRulesWorld, key: string, expected: string) {
  const params = currentParams(this) as unknown as Record<string, unknown>;
  expect(params[key]).to.equal(expected);
});

Then(
  'o parâmetro {string} deve ser o número {int}',
  function (this: BusinessRulesWorld, key: string, expected: number) {
    const params = currentParams(this) as unknown as Record<string, unknown>;
    expect(params[key]).to.equal(expected);
  }
);

Then('o parâmetro {string} não deve ser enviado', function (this: BusinessRulesWorld, key: string) {
  const params = currentParams(this) as unknown as Record<string, unknown>;
  expect(params[key], `o parâmetro "${key}" não deveria ir na query`).to.equal(undefined);
});

Then(
  'o parâmetro de lista {string} deve conter {int} itens',
  function (this: BusinessRulesWorld, key: string, expected: number) {
    const params = currentParams(this) as unknown as Record<string, unknown>;
    expect(params[key]).to.be.an('array').with.lengthOf(expected);
  }
);

Then(
  'o parâmetro de lista {string} deve conter {string}',
  function (this: BusinessRulesWorld, key: string, expected: string) {
    const params = currentParams(this) as unknown as Record<string, unknown>;
    expect(params[key]).to.be.an('array').that.includes(expected);
  }
);

Then('a chave de cache do recorte deve ser igual à guardada', function (this: BusinessRulesWorld) {
  const { savedKey } = dashboardData(this);
  expect(savedKey, 'nenhuma chave foi guardada').to.not.equal(undefined);
  expect(dashboardFiltersToKey(currentFilters(this))).to.equal(savedKey);
});

Then('a chave de cache do recorte deve ser diferente da guardada', function (this: BusinessRulesWorld) {
  const { savedKey } = dashboardData(this);
  expect(savedKey, 'nenhuma chave foi guardada').to.not.equal(undefined);
  expect(dashboardFiltersToKey(currentFilters(this))).to.not.equal(savedKey);
});
