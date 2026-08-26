import { When, Then } from '@cucumber/cucumber';
import { expect } from 'chai';
import { isValidCpf, isValidLoginIdentifier, maskCpf } from '@/shared/utils';
import { registerFormToRequest } from '@/features/auth/register/register-schema';
import { updateMyProfileRequestSchema } from '@/features/conta/service/conta-request-schema';
import type { BusinessRulesWorld } from '../../support/world';

When('eu aplico a máscara de CPF a {string}', function (this: BusinessRulesWorld, entrada: string) {
  this.result = maskCpf(entrada);
});

Then('o resultado da máscara de CPF deve ser {string}', function (this: BusinessRulesWorld, esperado: string) {
  expect(this.result).to.equal(esperado);
});

When('eu valido o CPF {string}', function (this: BusinessRulesWorld, cpf: string) {
  this.result = isValidCpf(cpf);
});

Then('o CPF deve ser considerado {string}', function (this: BusinessRulesWorld, validade: string) {
  expect(this.result).to.equal(validade === 'válido');
});

// Nome de usuário entra aqui como caso negativo de propósito: é a garantia de que ele não é
// aceito como credencial nem no formulário, antes mesmo de a requisição sair.
When('eu valido o identificador de login {string}', function (this: BusinessRulesWorld, identificador: string) {
  this.result = isValidLoginIdentifier(identificador);
});

Then('o identificador deve ser considerado {string}', function (this: BusinessRulesWorld, validade: string) {
  expect(this.result).to.equal(validade === 'válido');
});

When(
  'eu monto a atualização de perfil tentando enviar o CPF {string}',
  function (this: BusinessRulesWorld, cpf: string) {
    // O CPF entra como chave desconhecida de propósito: é o schema que tem de o descartar, não a tela.
    this.result = updateMyProfileRequestSchema.parse({
      username: 'candidato1',
      email: 'candidato@test.local',
      phoneNumber: '11988887777',
      cpf
    } as never);
  }
);

When('eu monto o cadastro com o CPF {string}', function (this: BusinessRulesWorld, cpf: string) {
  this.result = registerFormToRequest({
    username: 'candidato1',
    email: 'candidato@test.local',
    cpf,
    password: 'Abcd@123',
    passwordConfirmation: 'Abcd@123'
  });
});

Then('o corpo enviado não deve conter o campo {string}', function (this: BusinessRulesWorld, campo: string) {
  expect(this.result as Record<string, unknown>).to.not.have.property(campo);
});

Then('o corpo enviado deve conter o campo {string}', function (this: BusinessRulesWorld, campo: string) {
  expect(this.result as Record<string, unknown>).to.have.property(campo);
});

Then(
  'o corpo enviado deve conter o campo {string} com o valor {string}',
  function (this: BusinessRulesWorld, campo: string, valor: string) {
    expect(this.result as Record<string, unknown>).to.have.property(campo, valor);
  }
);
