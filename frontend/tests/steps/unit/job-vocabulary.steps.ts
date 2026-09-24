import {
  SALARY_RANGE_OPTIONS,
  experienceLevelVocabulary,
  jobAreaVocabulary,
  jobTypeVocabulary,
  workModelVocabulary,
  workShiftVocabulary
} from '@/shared/schema/job-vocabulary';
import {
  jobVocabularyResponseSchema,
  type JobVocabularyResponse
} from '@/features/vagas/service/jobs-feed-response-schema';
import { DataTable, Given, Then, When } from '@cucumber/cucumber';
import { expect } from 'chai';
import type { BusinessRulesWorld } from '../../support/world';

type Vocabulary = {
  options: readonly { value: string; label: string }[];
  normalize: (input: string | number | null | undefined) => string;
};

const VOCABULARIES: Record<string, Vocabulary> = {
  jobType: jobTypeVocabulary,
  workModel: workModelVocabulary,
  shift: workShiftVocabulary,
  experience: experienceLevelVocabulary,
  area: jobAreaVocabulary
};

function vocabularyByName(name: string): Vocabulary {
  const vocabulary = VOCABULARIES[name];
  expect(vocabulary, `vocabulário desconhecido: ${name}`).to.not.equal(undefined);
  return vocabulary;
}

type VocabularyWorldData = {
  vocabulary?: Vocabulary;
  normalized?: string;
};

function vocabularyData(world: BusinessRulesWorld): VocabularyWorldData {
  return world.data as VocabularyWorldData;
}

When('eu inspeciono o vocabulário {string}', function (this: BusinessRulesWorld, nome: string) {
  vocabularyData(this).vocabulary = vocabularyByName(nome);
});

When('eu normalizo {string} no vocabulário {string}', function (this: BusinessRulesWorld, valor: string, nome: string) {
  const vocabulary = vocabularyByName(nome);

  // O Gherkin só carrega texto; um valor puramente numérico representa o inteiro que os
  // endpoints antigos devolvem.
  const input = /^\d+$/.test(valor) ? Number(valor) : valor;
  vocabularyData(this).normalized = vocabulary.normalize(input);
});

Then('todos os valores devem ter rótulo', function (this: BusinessRulesWorld) {
  const { vocabulary } = vocabularyData(this);
  expect(vocabulary).to.not.equal(undefined);

  // Um identificador de enum não traduzido é reconhecível por ser PascalCase colado
  // (`HumanResources`, `MidLevel`, `OnSite`). Rótulos que coincidem com o nome do enum por
  // serem a mesma palavra em português - "Design", "Marketing", "Trainee" - são legítimos.
  const looksLikeEnumIdentifier = /[a-z][A-Z]/;

  vocabulary!.options.forEach((option) => {
    expect(option.label.trim(), `o valor "${option.value}" está sem rótulo`).to.not.equal('');
    expect(
      looksLikeEnumIdentifier.test(option.label),
      `o rótulo de "${option.value}" parece o identificador do enum, não um texto em português`
    ).to.equal(false);
  });
});

Then('nenhum rótulo deve repetir', function (this: BusinessRulesWorld) {
  const { vocabulary } = vocabularyData(this);
  const labels = vocabulary!.options.map((option) => option.label);

  expect(new Set(labels).size, `há rótulos repetidos em: ${labels.join(', ')}`).to.equal(labels.length);
});

Then('o valor normalizado deve ser {string}', function (this: BusinessRulesWorld, esperado: string) {
  expect(vocabularyData(this).normalized).to.equal(esperado);
});

When('eu inspeciono as faixas salariais', function (this: BusinessRulesWorld) {
  this.data.salaryRanges = SALARY_RANGE_OPTIONS;
});

Then('as faixas devem ser contínuas', function () {
  SALARY_RANGE_OPTIONS.forEach((range, index) => {
    if (index === 0) return;

    const previous = SALARY_RANGE_OPTIONS[index - 1];
    expect(range.min, `a faixa "${range.value}" não começa onde a anterior termina`).to.equal(previous.max);
  });
});

Then('a primeira faixa não deve ter piso', function () {
  expect(SALARY_RANGE_OPTIONS[0].min).to.equal(undefined);
});

Then('a última faixa não deve ter teto', function () {
  expect(SALARY_RANGE_OPTIONS[SALARY_RANGE_OPTIONS.length - 1].max).to.equal(undefined);
});

/** Resposta mínima válida de `GET /api/jobs/vocabulary`, sem o campo `cities`. */
function vocabularyResponseWithoutCities(): Record<string, unknown> {
  return {
    requirements: [{ label: 'Operação', items: ['Empilhadeira', 'WMS'] }],
    benefits: [{ label: 'Transporte', items: ['Fretado'] }],
    maxItemsPerJob: 20
  };
}

type VocabularyResponseWorldData = {
  rawVocabulary?: Record<string, unknown>;
  parsedVocabulary?: ReturnType<typeof jobVocabularyResponseSchema.safeParse>;
};

function vocabularyResponseData(world: BusinessRulesWorld): VocabularyResponseWorldData {
  return world.data as VocabularyResponseWorldData;
}

function acceptedVocabulary(world: BusinessRulesWorld): JobVocabularyResponse {
  const { parsedVocabulary } = vocabularyResponseData(world);
  expect(parsedVocabulary, 'a resposta do vocabulário ainda não foi validada').to.not.equal(undefined);
  expect(parsedVocabulary!.success, JSON.stringify(parsedVocabulary!.error?.issues)).to.equal(true);
  return parsedVocabulary!.data!;
}

function parseCityList(raw: string): string[] {
  return raw
    .split(',')
    .map((item) => item.trim())
    .filter((item) => item.length > 0);
}

Given('uma resposta do vocabulário de vagas sem o campo {string}', function (this: BusinessRulesWorld, campo: string) {
  const raw = vocabularyResponseWithoutCities();
  delete raw[campo];
  vocabularyResponseData(this).rawVocabulary = raw;
});

Given('uma resposta do vocabulário de vagas com as cidades:', function (this: BusinessRulesWorld, table: DataTable) {
  vocabularyResponseData(this).rawVocabulary = {
    ...vocabularyResponseWithoutCities(),
    cities: table.hashes().map((row) => ({ state: row.uf, items: parseCityList(row.cidades) }))
  };
});

When('eu valido a resposta do vocabulário de vagas', function (this: BusinessRulesWorld) {
  const data = vocabularyResponseData(this);
  data.parsedVocabulary = jobVocabularyResponseSchema.safeParse(data.rawVocabulary);
});

Then('a resposta do vocabulário deve ser aceita', function (this: BusinessRulesWorld) {
  acceptedVocabulary(this);
});

Then('o vocabulário validado deve ter {int} grupos de cidade', function (this: BusinessRulesWorld, total: number) {
  expect(acceptedVocabulary(this).cities).to.have.lengthOf(total);
});

Then(
  'o grupo de cidades da UF {string} deve listar {string}',
  function (this: BusinessRulesWorld, uf: string, cidades: string) {
    const group = acceptedVocabulary(this).cities.find((item) => item.state === uf);
    expect(group, `nenhum grupo de cidades para a UF "${uf}"`).to.not.equal(undefined);
    expect(group!.items).to.deep.equal(parseCityList(cidades));
  }
);

Then('o vocabulário validado deve manter os requisitos e benefícios da resposta', function (this: BusinessRulesWorld) {
  const vocabulary = acceptedVocabulary(this);
  const expected = vocabularyResponseWithoutCities();

  expect(vocabulary.requirements).to.deep.equal(expected.requirements);
  expect(vocabulary.benefits).to.deep.equal(expected.benefits);
  expect(vocabulary.maxItemsPerJob).to.equal(expected.maxItemsPerJob);
});
