import { Then } from '@cucumber/cucumber';
import { expect } from 'chai';
import { USER_TYPE_OPTIONS } from '@/shared/utils/lib/user-types';
import { enumValuesOf, listFilterData, listFilterScreen } from '../../support/list-filter-screens';
import type { BusinessRulesWorld } from '../../support/world';

Then(
  'as opções de {string} da tela {string} devem ser {string} seguido de todos os tipos de usuário',
  function (campo: string, tela: string, primeiro: string) {
    expect([...enumValuesOf(tela, campo)]).to.deep.equal([
      primeiro,
      ...USER_TYPE_OPTIONS.map((option) => option.value)
    ]);
  }
);

Then('devem existir {int} tipos de usuário para escolher', function (total: number) {
  expect(USER_TYPE_OPTIONS).to.have.lengthOf(total);
});

Then('o tipo de usuário {string} deve ser oferecido com o rótulo {string}', function (valor: string, rotulo: string) {
  const option = USER_TYPE_OPTIONS.find((item) => item.value === valor);
  expect(option, `o tipo "${valor}" não está entre as opções do select`).to.not.equal(undefined);
  expect(option!.label).to.equal(rotulo);
});

Then('o tipo de usuário {string} deve ser aceito pelo filtro da tela {string}', function (valor: string, tela: string) {
  const screen = listFilterScreen(tela);
  expect(screen.isValid({ ...screen.defaults, userType: valor })).to.equal(true);
});

Then(
  'o parâmetro {string} da listagem não deve ser o rótulo {string}',
  function (this: BusinessRulesWorld, campo: string, rotulo: string) {
    const { params } = listFilterData(this);
    expect(params, 'o filtro ainda não foi convertido em parâmetros').to.not.equal(undefined);
    expect(params![campo]).to.not.equal(rotulo);
  }
);
