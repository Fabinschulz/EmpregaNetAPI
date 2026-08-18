import { Then, When } from '@cucumber/cucumber';
import { createPaginatedResponseSchema, userResponseSchema, type UserResponse } from '@/shared/schema';
import { expect } from 'chai';
import { getByPath } from '../../support/object-path';
import type { BusinessRulesWorld } from '../../support/world';

/** Payload íntegro devolvido pela API; cada cenário degrada um campo a partir dele. */
function validUserPayload(): Record<string, unknown> {
  return {
    id: 7,
    username: 'ana.souza',
    email: 'ana.souza@empreganet.com.br',
    phoneNumber: '11988887777',
    userType: 'Candidato',
    roles: ['Candidate'],
    isDeleted: false,
    createdAt: '2026-01-10T12:00:00+00:00',
    updatedAt: '2026-02-01T09:30:00+00:00',
    deletedAt: null
  };
}

function readUser(this: BusinessRulesWorld, payload: unknown): void {
  const parsed = userResponseSchema.safeParse(payload);
  this.result = parsed.success ? parsed.data : undefined;
  this.error = parsed.success ? undefined : parsed.error;
}

function parsedUser(world: BusinessRulesWorld): UserResponse {
  return world.result as UserResponse;
}

When(
  'eu leio a resposta de usuário com {string} vazio e {string} vazio',
  function (this: BusinessRulesWorld, primeiro: string, segundo: string) {
    readUser.call(this, { ...validUserPayload(), [primeiro]: '', [segundo]: '' });
  }
);

When('eu leio a resposta de usuário com {string} nulo', function (this: BusinessRulesWorld, campo: string) {
  readUser.call(this, { ...validUserPayload(), [campo]: null });
});

When(
  'eu leio a resposta de usuário sem {string} e sem {string}',
  function (this: BusinessRulesWorld, primeiro: string, segundo: string) {
    const payload = validUserPayload();
    delete payload[primeiro];
    delete payload[segundo];
    readUser.call(this, payload);
  }
);

When('eu leio a resposta de usuário sem {string}', function (this: BusinessRulesWorld, campo: string) {
  const payload = validUserPayload();
  delete payload[campo];
  readUser.call(this, payload);
});

When(
  'eu leio uma listagem de usuários em que o segundo registro tem {string} vazio',
  function (this: BusinessRulesWorld, campo: string) {
    const listSchema = createPaginatedResponseSchema(userResponseSchema);
    const parsed = listSchema.safeParse({
      page: 1,
      totalPages: 1,
      totalItems: 3,
      data: [validUserPayload(), { ...validUserPayload(), id: 8, [campo]: '' }, { ...validUserPayload(), id: 9 }]
    });

    this.result = parsed.success ? parsed.data : undefined;
    this.error = parsed.success ? undefined : parsed.error;
  }
);

Then('a leitura da resposta deve ter sucesso', function (this: BusinessRulesWorld) {
  expect(this.error, 'esperava um parse bem-sucedido').to.equal(undefined);
});

Then('a leitura da resposta deve falhar', function (this: BusinessRulesWorld) {
  expect(this.error, 'esperava um erro de contrato').to.not.equal(undefined);
});

Then(
  'o usuário lido deve ter {string} igual a {string}',
  function (this: BusinessRulesWorld, campo: string, esperado: string) {
    expect(getByPath(parsedUser(this) as unknown as Record<string, unknown>, campo)).to.equal(esperado);
  }
);

Then('o usuário lido deve ter {string} nulo', function (this: BusinessRulesWorld, campo: string) {
  expect(getByPath(parsedUser(this) as unknown as Record<string, unknown>, campo)).to.equal(null);
});

Then('o usuário lido deve ter {string} vazio', function (this: BusinessRulesWorld, campo: string) {
  expect(getByPath(parsedUser(this) as unknown as Record<string, unknown>, campo)).to.deep.equal([]);
});

Then('o usuário lido não deve estar excluído', function (this: BusinessRulesWorld) {
  expect(parsedUser(this).isDeleted).to.equal(false);
});

Then('a listagem lida deve conter {int} usuários', function (this: BusinessRulesWorld, total: number) {
  const list = this.result as { data: UserResponse[] };
  expect(list.data).to.have.length(total);
});
