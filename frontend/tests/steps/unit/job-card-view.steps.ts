import { Given, Then, When } from '@cucumber/cucumber';
import { toJobFacts, type JobFact } from '@/features/vagas/feed/job-card/job-facts';
import { toJobTags } from '@/features/vagas/feed/job-card/job-tags';
import type { JobFeedItemResponse } from '@/features/vagas/service/jobs-feed-response-schema';
import type { CardTag } from '@/shared/components/ui/molecules/entity-card/card-tags';
import { expect } from 'chai';
import type { BusinessRulesWorld } from '../../support/world';

/** Vaga íntegra; cada cenário degrada um campo a partir dela. */
function completeJob(): JobFeedItemResponse {
  return {
    id: 1,
    title: 'Auxiliar de Produção',
    summary: 'Atuação em linha de montagem.',
    company: { id: 1, name: 'Acme', logoUrl: null },
    location: { city: 'Extrema', state: 'MG', country: 'BR' },
    salary: { min: 2300, max: 2800, disclosed: true },
    jobType: 'Clt',
    workModel: 'OnSite',
    workShift: 'PrimeiroTurno',
    experienceLevel: 'SemExperiencia',
    area: 'Logistica',
    isPcdFriendly: false,
    requirements: ['Empilhadeira'],
    benefits: ['Fretado'],
    publishedAt: '2026-07-18T12:00:00+00:00',
    applicationsCount: 0,
    isActive: true
  };
}

function job(world: BusinessRulesWorld): JobFeedItemResponse {
  return world.data.job as JobFeedItemResponse;
}

function facts(world: BusinessRulesWorld): JobFact[] {
  return world.result as JobFact[];
}

function tags(world: BusinessRulesWorld): CardTag[] {
  return world.result as CardTag[];
}

function parseList(raw: string): string[] {
  return raw === '' ? [] : raw.split(',');
}

Given('uma vaga do feed', function (this: BusinessRulesWorld) {
  this.data.job = completeJob();
});

Given('uma vaga sem características', function (this: BusinessRulesWorld) {
  this.data.job = {
    ...completeJob(),
    workShift: 'NaoSelecionado',
    experienceLevel: 'NaoSelecionado',
    area: 'NaoSelecionado',
    requirements: [],
    benefits: []
  };
});

Given('a vaga tem {string} como {string}', function (this: BusinessRulesWorld, campo: string, valor: string) {
  this.data.job = { ...job(this), [campo]: valor };
});

Given('a vaga tem a UF como {string}', function (this: BusinessRulesWorld, valor: string) {
  const current = job(this);
  this.data.job = { ...current, location: { ...current.location, state: valor } };
});

Given('a vaga não divulga o salário', function (this: BusinessRulesWorld) {
  this.data.job = { ...job(this), salary: { min: null, max: null, disclosed: false } };
});

Given('a vaga é afirmativa para PcD', function (this: BusinessRulesWorld) {
  this.data.job = { ...job(this), isPcdFriendly: true };
});

When('eu monto a banda de dados do cartão', function (this: BusinessRulesWorld) {
  this.result = toJobFacts(job(this));
});

When('eu monto as etiquetas do cartão', function (this: BusinessRulesWorld) {
  this.result = toJobTags(job(this));
});

Then('os dados do cartão devem ser {string}', function (this: BusinessRulesWorld, esperado: string) {
  expect(facts(this).map((fact) => fact.key)).to.deep.equal(parseList(esperado));
});

Then('os dados do cartão não devem incluir {string}', function (this: BusinessRulesWorld, chave: string) {
  expect(facts(this).map((fact) => fact.key)).to.not.include(chave);
});

Then('os dados do cartão devem incluir {string}', function (this: BusinessRulesWorld, chave: string) {
  expect(facts(this).map((fact) => fact.key)).to.include(chave);
});

Then(
  'o rótulo de {string} deve ser {string}',
  function (this: BusinessRulesWorld, chave: string, esperado: string) {
    expect(facts(this).find((fact) => fact.key === chave)?.label).to.equal(esperado);
  }
);

Then('o dado {string} deve estar destacado', function (this: BusinessRulesWorld, chave: string) {
  expect(facts(this).find((fact) => fact.key === chave)?.strong).to.equal(true);
});

Then('o dado {string} não deve estar destacado', function (this: BusinessRulesWorld, chave: string) {
  expect(facts(this).find((fact) => fact.key === chave)?.strong).to.equal(false);
});

Then('as etiquetas do cartão devem ser {string}', function (this: BusinessRulesWorld, esperado: string) {
  expect(tags(this).map((tag) => tag.label)).to.deep.equal(parseList(esperado));
});

Then('as etiquetas destacadas do cartão devem ser {string}', function (this: BusinessRulesWorld, esperado: string) {
  const accented = tags(this).filter((tag) => tag.tone === 'accent');
  expect(accented.map((tag) => tag.label)).to.deep.equal(parseList(esperado));
});
