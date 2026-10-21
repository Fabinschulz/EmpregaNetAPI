import { Given, Then, When } from '@cucumber/cucumber';
import { expect } from 'chai';
import { getPublicEnv, type PublicEnv } from '@/shared/utils';
import type { BusinessRulesWorld } from '../../support/world';

type EnvOverrides = Record<string, string | undefined>;

function overrides(world: BusinessRulesWorld): EnvOverrides {
  return (world.data.env ??= {}) as EnvOverrides;
}

Given('que {word} não está definida', function (this: BusinessRulesWorld, name: string) {
  overrides(this)[name] = undefined;
});

Given('que {word} está definida como {string}', function (this: BusinessRulesWorld, name: string, value: string) {
  overrides(this)[name] = value;
});

When('eu leio o ambiente público', function (this: BusinessRulesWorld) {
  const previous: EnvOverrides = {};
  for (const [name, value] of Object.entries(overrides(this))) {
    previous[name] = process.env[name];
    if (value === undefined) delete process.env[name];
    else process.env[name] = value;
  }

  try {
    this.result = getPublicEnv();
  } catch (error) {
    this.error = error;
  } finally {
    for (const [name, value] of Object.entries(previous)) {
      if (value === undefined) delete process.env[name];
      else process.env[name] = value;
    }
  }
});

Then('a leitura deve passar', function (this: BusinessRulesWorld) {
  expect(this.error, 'a leitura não deveria falhar').to.equal(undefined);
});

Then('a leitura deve falhar com {string}', function (this: BusinessRulesWorld, fragment: string) {
  expect(this.error).to.be.instanceOf(Error);
  expect((this.error as Error).message).to.contain(fragment);
});

Then('o client id do Google deve ser ausente', function (this: BusinessRulesWorld) {
  expect((this.result as PublicEnv).NEXT_PUBLIC_GOOGLE_CLIENT_ID).to.equal(undefined);
});

Then('o client id do Google deve ser {string}', function (this: BusinessRulesWorld, expected: string) {
  expect((this.result as PublicEnv).NEXT_PUBLIC_GOOGLE_CLIENT_ID).to.equal(expected);
});
