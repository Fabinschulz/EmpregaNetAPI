import { Then, When } from '@cucumber/cucumber';
import { expect } from 'chai';
import { APPLICATION_STATUSES, applicationStatusLabels } from '@/features/candidaturas/domain';
import {
  currentScreen,
  currentValues,
  enumValuesOf,
  listFilterData,
  listFilterScreen
} from '../../support/list-filter-screens';
import type { BusinessRulesWorld } from '../../support/world';

function currentParams(world: BusinessRulesWorld): Record<string, unknown> {
  const { params } = listFilterData(world);
  expect(params, 'o filtro ainda não foi convertido em parâmetros').to.not.equal(undefined);
  return params!;
}

When('eu converto o filtro em parâmetros da listagem', function (this: BusinessRulesWorld) {
  listFilterData(this).params = currentScreen(this).toParams(currentValues(this));
});

Then('o filtro não deve ser aceito pelo schema da tela', function (this: BusinessRulesWorld) {
  expect(currentScreen(this).isValid(currentValues(this))).to.equal(false);
});

Then(
  'as opções de {string} da tela {string} devem ser {string} seguido de todos os status de candidatura',
  function (campo: string, tela: string, primeiro: string) {
    const screen = listFilterScreen(tela);
    const values = enumValuesOf(tela, campo);

    expect([...values]).to.deep.equal([primeiro, ...APPLICATION_STATUSES]);

    // Cada opção precisa passar pelo schema do formulário; uma opção recusada seria um item do
    // select que, escolhido, invalida o filtro inteiro.
    values.forEach((value) => {
      expect(screen.isValid({ ...screen.defaults, [campo]: value }), `"${value}" recusado pelo schema`).to.equal(true);
    });
  }
);

Then('todo status de candidatura deve ter rótulo em português', function () {
  APPLICATION_STATUSES.forEach((status) => {
    const label = applicationStatusLabels[status];
    expect(label?.trim(), `o status "${status}" está sem rótulo`).to.not.equal(undefined);
    expect(label.trim(), `o status "${status}" está sem rótulo`).to.not.equal('');
    expect(label, `o rótulo de "${status}" é o nome cru do enum`).to.not.equal(status);
  });
});

Then(
  'o parâmetro {string} da listagem deve ser {string}',
  function (this: BusinessRulesWorld, campo: string, esperado: string) {
    expect(currentParams(this)[campo]).to.equal(esperado);
  }
);

Then(
  'o parâmetro {string} da listagem deve ser o booleano {string}',
  function (this: BusinessRulesWorld, campo: string, esperado: string) {
    expect(currentParams(this)[campo]).to.equal(esperado === 'true');
  }
);

Then('o parâmetro {string} da listagem deve estar ausente', function (this: BusinessRulesWorld, campo: string) {
  expect(currentParams(this)[campo], `o parâmetro "${campo}" não deveria ir na query`).to.equal(undefined);
});
