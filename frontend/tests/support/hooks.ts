import { After, Before } from '@cucumber/cucumber';
import { installHttpDoubles, restoreHttp } from './axios-double';
import type { BusinessRulesWorld } from './world';

/** Garante que cada cenário comece com o World limpo, mesmo em execução paralela. */
Before(function (this: BusinessRulesWorld) {
  this.reset();
  installHttpDoubles();
});

/**
 * Os duplos de `axiosApi` são restaurados no fim de cada cenário: `axiosApi` é módulo único
 * do processo, e um duplo que lhe sobreviva faria um teste futuro passar contra a resposta
 * declarada aqui em vez da chamada real.
 */
After(function () {
  restoreHttp();
});
