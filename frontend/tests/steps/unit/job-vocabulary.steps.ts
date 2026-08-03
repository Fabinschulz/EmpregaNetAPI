import {
    SALARY_RANGE_OPTIONS,
    experienceLevelVocabulary,
    jobAreaVocabulary,
    jobTypeVocabulary,
    workModelVocabulary,
    workShiftVocabulary
} from '@/shared/schema/job-vocabulary';
import { Then, When } from '@cucumber/cucumber';
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

When(
  'eu normalizo {string} no vocabulário {string}',
  function (this: BusinessRulesWorld, valor: string, nome: string) {
    const vocabulary = vocabularyByName(nome);

    // O Gherkin só carrega texto; um valor puramente numérico representa o inteiro que os
    // endpoints antigos devolvem.
    const input = /^\d+$/.test(valor) ? Number(valor) : valor;
    vocabularyData(this).normalized = vocabulary.normalize(input);
  }
);

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
