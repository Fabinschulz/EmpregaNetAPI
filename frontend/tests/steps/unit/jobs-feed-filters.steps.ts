import { DataTable, Given, Then, When } from '@cucumber/cucumber';
import { expect } from 'chai';
import {
  cityGroupsForStates,
  type CityGroupByState,
  type CityGroupOption
} from '@/features/vagas/feed/filters/city-groups';
import {
  countActiveJobsFeedFilters,
  jobsFeedFiltersToApiParams,
  jobsFeedFiltersToSearchParams,
  parseJobsFeedFilters,
  type JobsFeedFilters
} from '@/features/vagas/feed/filters/jobs-feed-filters';
import type { JobsFeedQueryParams } from '@/features/vagas/service/jobs-feed-params';
import { UF_SELECT_OPTIONS } from '@/shared/schema';
import type { BusinessRulesWorld } from '../../support/world';
import { getByPath } from '../../support/object-path';

type FeedWorldData = {
  url?: string;
  filters?: JobsFeedFilters;
  serialized?: string;
  apiParams?: JobsFeedQueryParams;
  cities?: CityGroupByState[];
  cityGroups?: CityGroupOption[];
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

/**
 * Reproduz o que uma pill de filtro faz ao ser clicada: alterna um valor numa lista de filtro.
 *
 * O componente em si não é coberto aqui - a suite não tem renderizador de React -, mas este é o
 * contrato de que ele depende: alternar, refletir na URL e contar como filtro ativo.
 */
When(
  'eu alterno o valor {string} no filtro {string}',
  function (this: BusinessRulesWorld, valor: string, campo: string) {
    const filters = currentFilters(this);
    const current = filters[campo as keyof JobsFeedFilters] as readonly string[];

    expect(Array.isArray(current), `o filtro "${campo}" não é uma lista`).to.equal(true);

    const next = current.includes(valor) ? current.filter((item) => item !== valor) : [...current, valor];

    feedData(this).filters = { ...filters, [campo]: next } as JobsFeedFilters;
  }
);

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

/** Recarregar a página: o feed lê de novo a query string que ele mesmo escreveu. */
When('eu interpreto de novo os filtros a partir da URL serializada', function (this: BusinessRulesWorld) {
  const { serialized } = feedData(this);
  expect(serialized, 'os filtros ainda não foram serializados').to.not.equal(undefined);
  feedData(this).filters = parseJobsFeedFilters(new URLSearchParams(serialized));
});

/** Lista separada por vírgula do Gherkin: `""` é lista vazia, não uma lista com um item vazio. */
function parseList(raw: string): string[] {
  return raw
    .split(',')
    .map((item) => item.trim())
    .filter((item) => item.length > 0);
}

function currentCityGroups(world: BusinessRulesWorld): CityGroupOption[] {
  const { cityGroups } = feedData(world);
  expect(cityGroups, 'os grupos de cidade ainda não foram calculados').to.not.equal(undefined);
  return cityGroups!;
}

function cityGroupByLabel(world: BusinessRulesWorld, label: string): CityGroupOption | undefined {
  return currentCityGroups(world).find((group) => group.label === label);
}

Given('que o vocabulário do feed traz as cidades:', function (this: BusinessRulesWorld, table: DataTable) {
  feedData(this).cities = table.hashes().map((row) => ({ state: row.uf, items: parseList(row.cidades) }));
});

Given('que o vocabulário do feed não traz cidades', function (this: BusinessRulesWorld) {
  feedData(this).cities = [];
});

Given('que o vocabulário do feed traz uma cidade em cada UF do seletor de Estado', function (this: BusinessRulesWorld) {
  feedData(this).cities = UF_SELECT_OPTIONS.map((option) => ({
    state: option.value,
    items: [`Cidade ${option.value}`]
  }));
});

When('eu calculo os grupos de cidade para os Estados {string}', function (this: BusinessRulesWorld, estados: string) {
  const { cities } = feedData(this);
  expect(cities, 'o vocabulário de cidades não foi montado no cenário').to.not.equal(undefined);
  feedData(this).cityGroups = cityGroupsForStates(cities!, parseList(estados));
});

Then('devem aparecer {int} grupos de cidade', function (this: BusinessRulesWorld, total: number) {
  const groups = currentCityGroups(this);
  expect(groups, `grupos: ${groups.map((group) => group.label).join(', ')}`).to.have.lengthOf(total);
});

Then(
  'o grupo de cidade {string} deve listar {string}',
  function (this: BusinessRulesWorld, rotulo: string, cidades: string) {
    const group = cityGroupByLabel(this, rotulo);
    expect(group, `nenhum grupo de cidade com o rótulo "${rotulo}"`).to.not.equal(undefined);
    expect(group!.items).to.deep.equal(parseList(cidades));
  }
);

Then('não deve aparecer o grupo de cidade {string}', function (this: BusinessRulesWorld, rotulo: string) {
  expect(cityGroupByLabel(this, rotulo)).to.equal(undefined);
});

Then('cada grupo de cidade deve ter o nome por extenso da sua UF', function (this: BusinessRulesWorld) {
  const groups = currentCityGroups(this);

  // Sem Estado selecionado os grupos saem na ordem do vocabulário, que aqui segue o seletor.
  UF_SELECT_OPTIONS.forEach((option, index) => {
    const { label } = groups[index];
    expect(label.trim(), `a UF "${option.value}" ficou sem rótulo no grupo de cidade`).to.not.equal('');
    expect(label, `o grupo da UF "${option.value}" foi rotulado com o código cru`).to.not.equal(option.value);
    // O mesmo nome que o seletor de Estado mostra ("CE - Ceará"), para a gaveta não se contradizer.
    expect(option.label, `o grupo da UF "${option.value}" diverge do seletor de Estado`).to.equal(
      `${option.value} - ${label}`
    );
  });
});
