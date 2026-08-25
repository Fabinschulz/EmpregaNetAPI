import { Then, When } from '@cucumber/cucumber';
import { expect } from 'chai';
import {
  formatCompact,
  formatCount,
  formatDays,
  formatKpiValue,
  formatPercent,
  formatSignedPercent
} from '@/features/dashboard/analytics/shared/dashboard-format';
import type { BusinessRulesWorld } from '../../support/world';

When('eu formato a contagem {float}', function (this: BusinessRulesWorld, value: number) {
  this.result = formatCount(value);
});

When('eu formato a contagem compacta {float}', function (this: BusinessRulesWorld, value: number) {
  this.result = formatCompact(value);
});

When('eu formato a percentagem {float}', function (this: BusinessRulesWorld, value: number) {
  this.result = formatPercent(value);
});

When('eu formato a variação {float}', function (this: BusinessRulesWorld, value: number) {
  this.result = formatSignedPercent(value);
});

When('eu formato o indicador {float} com unidade {string}', function (this: BusinessRulesWorld, value: number, unit: string) {
  this.result = formatKpiValue(value, unit);
});

When('eu formato o indicador {float} sem unidade', function (this: BusinessRulesWorld, value: number) {
  this.result = formatKpiValue(value, null);
});

When('eu formato {float} dias', function (this: BusinessRulesWorld, value: number) {
  this.result = formatDays(value);
});

Then('o número formatado deve ser {string}', function (this: BusinessRulesWorld, expected: string) {
  // Normaliza o espaço estreito que o Intl insere entre número e sufixo ("1,2 mil"): o valor
  // exibido é o certo, mas o caractere não é o espaço comum que a tabela do cenário contém.
  const actual = String(this.result).replace(/ | /g, ' ');
  expect(actual).to.equal(expected);
});
