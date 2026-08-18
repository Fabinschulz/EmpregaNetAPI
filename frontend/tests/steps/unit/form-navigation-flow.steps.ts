import { adminUsersRoutes } from '@/features/admin/usuarios/admin-users-routes';
import { companiesRoutes } from '@/features/admin/empresas/companies-routes';
import { jobsRoutes } from '@/features/recrutamento/vagas/jobs-routes';
import { createdIdResponseSchema } from '@/shared/schema';
import { Then, When } from '@cucumber/cucumber';
import { expect } from 'chai';
import type { BusinessRulesWorld } from '../../support/world';

type EntityRoutes = { list: string; detail: (id: number) => string };

const ROUTES: Record<string, EntityRoutes> = {
  empresa: companiesRoutes,
  vaga: jobsRoutes,
  usuario: adminUsersRoutes
};

function readCreated(this: BusinessRulesWorld, payload: unknown): void {
  const parsed = createdIdResponseSchema.safeParse(payload);
  this.result = parsed.success ? parsed.data : undefined;
  this.error = parsed.success ? undefined : parsed.error;
}

When('eu leio a resposta de criação com id {int}', function (this: BusinessRulesWorld, id: number) {
  readCreated.call(this, id);
});

When('eu leio uma resposta de criação vazia', function (this: BusinessRulesWorld) {
  readCreated.call(this, null);
});

When('eu leio uma resposta de criação com o corpo antigo', function (this: BusinessRulesWorld) {
  readCreated.call(this, { id: 42, message: 'Recurso criado com sucesso. ID: 42' });
});

Then('a leitura da criação deve ter sucesso', function (this: BusinessRulesWorld) {
  expect(this.error, 'esperava um parse bem-sucedido').to.equal(undefined);
});

Then('a leitura da criação deve falhar', function (this: BusinessRulesWorld) {
  expect(this.error, 'esperava um erro de contrato').to.not.equal(undefined);
});

Then('o id da entidade criada deve ser {int}', function (this: BusinessRulesWorld, esperado: number) {
  expect(this.result).to.equal(esperado);
});

When(
  'eu resolvo a rota de detalhe de {string} para o id {int}',
  function (this: BusinessRulesWorld, entidade: string, id: number) {
    this.result = ROUTES[entidade].detail(id);
  }
);

When('eu resolvo a rota de listagem de {string}', function (this: BusinessRulesWorld, entidade: string) {
  this.result = ROUTES[entidade].list;
});

Then('a rota resolvida deve ser {string}', function (this: BusinessRulesWorld, esperada: string) {
  expect(this.result).to.equal(esperada);
});
