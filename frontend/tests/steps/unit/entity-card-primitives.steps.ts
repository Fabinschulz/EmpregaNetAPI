import { Then, When } from '@cucumber/cucumber';
// Importados dos módulos puros e não do barrel de componentes: o barrel arrasta ficheiros que
// importam `.module.scss`, e o runner do Cucumber não resolve essa extensão.
import { toCardTags } from '@/shared/components/ui/molecules/entity-card/card-tags';
import { entityInitials } from '@/shared/components/ui/molecules/entity-card/entity-initials';
import { expect } from 'chai';
import type { BusinessRulesWorld } from '../../support/world';

/**
 * O Gherkin passa a lista como texto separado por vírgula. Uma célula vazia representa a lista
 * vazia, e o split preserva de propósito os itens em branco — é justamente o descarte deles que
 * está sob teste, por isso não se pode filtrar aqui.
 */
function parseList(raw: string): string[] {
  return raw === '' ? [] : raw.split(',');
}

When('eu monto etiquetas a partir de {string}', function (this: BusinessRulesWorld, entrada: string) {
  this.result = toCardTags(parseList(entrada));
});

When('eu peço as iniciais de {string}', function (this: BusinessRulesWorld, nome: string) {
  this.result = entityInitials(nome);
});

Then('as etiquetas devem ser {string}', function (this: BusinessRulesWorld, esperado: string) {
  const tags = this.result as { label: string }[];
  expect(tags.map((tag) => tag.label)).to.deep.equal(parseList(esperado));
});

Then('as iniciais devem ser {string}', function (this: BusinessRulesWorld, esperado: string) {
  expect(this.result).to.equal(esperado);
});
