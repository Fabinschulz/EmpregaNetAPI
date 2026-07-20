import { Before } from '@cucumber/cucumber';
import type { BusinessRulesWorld } from './world';

/** Garante que cada cenário comece com o World limpo, mesmo em execução paralela. */
Before(function (this: BusinessRulesWorld) {
  this.reset();
});
