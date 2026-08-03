import { Given, Then, When } from '@cucumber/cucumber';
import { expect } from 'chai';
import {
  countActiveJobsFeedFilters,
  jobsFeedFiltersToApiParams,
  jobsFeedFiltersToSearchParams,
  parseJobsFeedFilters,
  type JobsFeedFilters,
  type JobsFeedQueryParams
} from '@/features/vagas/service/jobs-feed-schema';
import type { BusinessRulesWorld } from '../../support/world';
import { getByPath } from '../../support/object-path';

type FeedWorldData = {
  url?: string;
  filters?: JobsFeedFilters;
  serialized?: string;
  apiParams?: JobsFeedQueryParams;
};

function feedData(world: BusinessRulesWorld): FeedWorldData {
  return world.data as FeedWorldData;
}

function currentFilters(world: BusinessRulesWorld): JobsFeedFilters {
  const { filters } = feedData(world);
  expect(filters, 'os filtros ainda não foram interpretados').to.not.equal(undefined);
  return filters!;
}

Given('que a URL do feed é {string}', function (this: BusinessRulesWorld, url: string) {
  feedData(this).url = url;
});

When('eu interpreto os filtros do feed', function (this: BusinessRulesWorld) {
  const url = feedData(this).url ?? '';
  feedData(this).filters = parseJobsFeedFilters(new URLSearchParams(url));
});

When('eu serializo os filtros de volta para a URL', function (this: BusinessRulesWorld) {
  feedData(this).serialized = jobsFeedFiltersToSearchParams(currentFilters(this)).toString();
});

When('eu converto os filtros em parâmetros da API', function (this: BusinessRulesWorld) {
  feedData(this).apiParams = jobsFeedFiltersToApiParams(currentFilters(this), 1);
});

Then('o filtro {string} deve ser {string}', function (this: BusinessRulesWorld, campo: string, esperado: string) {
  const actual = getByPath(currentFilters(this) as unknown as Record<string, unknown>, campo);
  expect(String(actual ?? '')).to.equal(esperado);
});

Then('a lista {string} deve conter {int} itens', function (this: BusinessRulesWorld, campo: string, total: number) {
  const actual = getByPath(currentFilters(this) as unknown as Record<string, unknown>, campo);
  expect(Array.isArray(actual), `esperava que "${campo}" fosse uma lista`).to.equal(true);
  expect((actual as unknown[]).length).to.equal(total);
});

Then('a lista {string} deve conter {string}', function (this: BusinessRulesWorld, campo: string, valor: string) {
  const actual = getByPath(currentFilters(this) as unknown as Record<string, unknown>, campo);
  expect(Array.isArray(actual), `esperava que "${campo}" fosse uma lista`).to.equal(true);
  expect(actual as unknown[]).to.include(valor);
});

Then('nenhum filtro deve estar ativo', function (this: BusinessRulesWorld) {
  expect(countActiveJobsFeedFilters(currentFilters(this))).to.equal(0);
});

Then('devem estar ativos {int} filtros', function (this: BusinessRulesWorld, total: number) {
  expect(countActiveJobsFeedFilters(currentFilters(this))).to.equal(total);
});

Then('a URL serializada deve ser {string}', function (this: BusinessRulesWorld, esperado: string) {
  const { serialized } = feedData(this);
  // Decodifica para comparar com o Gherkin legível: `.NET` e vírgulas escapadas atrapalhariam
  // a leitura do cenário sem acrescentar nada ao que se quer verificar.
  expect(decodeURIComponent(serialized ?? '')).to.equal(esperado);
});

Then(
  'o parâmetro {string} da API deve ser {string}',
  function (this: BusinessRulesWorld, campo: string, esperado: string) {
    const params = feedData(this).apiParams;
    expect(params, 'os parâmetros da API ainda não foram construídos').to.not.equal(undefined);

    const actual = getByPath(params as unknown as Record<string, unknown>, campo);
    expect(actual === undefined ? '' : String(actual)).to.equal(esperado);
  }
);

Then('o parâmetro {string} da API deve estar ausente', function (this: BusinessRulesWorld, campo: string) {
  const params = feedData(this).apiParams;
  expect(params, 'os parâmetros da API ainda não foram construídos').to.not.equal(undefined);
  expect(getByPath(params as unknown as Record<string, unknown>, campo)).to.equal(undefined);
});
