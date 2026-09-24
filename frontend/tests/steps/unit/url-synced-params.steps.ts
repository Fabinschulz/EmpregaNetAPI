import { Given, Then, When } from '@cucumber/cucumber';
import { expect } from 'chai';
import { currentScreen, currentValues, listFilterData, listFilterScreen } from '../../support/list-filter-screens';
import type { BusinessRulesWorld } from '../../support/world';

/** Query legível para comparar com o Gherkin: `URLSearchParams` codifica espaço como `+`. */
function readableQuery(params: URLSearchParams): string {
  return decodeURIComponent(params.toString().replace(/\+/g, ' '));
}

Given('que o filtro da tela {string} está nos valores padrão', function (this: BusinessRulesWorld, tela: string) {
  const screen = listFilterScreen(tela);
  listFilterData(this).screen = screen;
  listFilterData(this).values = { ...screen.defaults };
});

Given('o campo {string} do filtro vale {string}', function (this: BusinessRulesWorld, campo: string, valor: string) {
  const values = currentValues(this);
  expect(Object.keys(currentScreen(this).defaults), `o filtro não tem o campo "${campo}"`).to.include(campo);
  values[campo] = valor;
});

Given('que a URL da tela {string} é {string}', function (this: BusinessRulesWorld, tela: string, url: string) {
  listFilterData(this).screen = listFilterScreen(tela);
  listFilterData(this).query = url;
});

Given(
  'que a URL da tela {string} traz o campo {string} com {int} caracteres',
  function (this: BusinessRulesWorld, tela: string, campo: string, tamanho: number) {
    listFilterData(this).screen = listFilterScreen(tela);
    listFilterData(this).query = new URLSearchParams({ [campo]: 'a'.repeat(tamanho) }).toString();
  }
);

When('eu leio o filtro da URL', function (this: BusinessRulesWorld) {
  const data = listFilterData(this);
  data.values = currentScreen(this).parseUrl(data.query ?? '');
});

When('eu gravo o filtro na URL', function (this: BusinessRulesWorld) {
  listFilterData(this).serialized = currentScreen(this).serialize(currentValues(this));
});

/** O que acontece ao recarregar: a tela monta de novo e lê o filtro da query string gravada. */
When('eu recarrego a tela com a URL gravada', function (this: BusinessRulesWorld) {
  const data = listFilterData(this);
  expect(data.serialized, 'nada foi gravado na URL ainda').to.not.equal(undefined);
  data.values = currentScreen(this).parseUrl(data.serialized!.toString());
});

Then('o filtro deve ser aceito pelo schema da tela', function (this: BusinessRulesWorld) {
  expect(currentScreen(this).isValid(currentValues(this))).to.equal(true);
});

Then('o filtro deve estar nos valores padrão da tela', function (this: BusinessRulesWorld) {
  expect(currentValues(this)).to.deep.equal(currentScreen(this).defaults);
});

Then(
  'o campo {string} do filtro deve ser {string}',
  function (this: BusinessRulesWorld, campo: string, esperado: string) {
    expect(currentValues(this)[campo]).to.equal(esperado);
  }
);

Then(
  'o campo {string} do filtro deve ter {int} caracteres',
  function (this: BusinessRulesWorld, campo: string, esperado: number) {
    const value = currentValues(this)[campo];
    expect(value).to.be.a('string');
    expect(String(value).length).to.equal(esperado);
  }
);

Then('a URL gravada deve ser {string}', function (this: BusinessRulesWorld, esperado: string) {
  const { serialized } = listFilterData(this);
  expect(serialized, 'nada foi gravado na URL ainda').to.not.equal(undefined);
  expect(readableQuery(serialized!)).to.equal(esperado);
});

Then('o filtro deve contar como ativo', function (this: BusinessRulesWorld) {
  expect(currentScreen(this).hasActive(currentValues(this))).to.equal(true);
});

Then('o filtro não deve contar como ativo', function (this: BusinessRulesWorld) {
  expect(currentScreen(this).hasActive(currentValues(this))).to.equal(false);
});
