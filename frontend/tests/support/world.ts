import { setWorldConstructor, World as CucumberWorld } from '@cucumber/cucumber';

/**
 * World customizado: um "saco" de contexto tipado que cada cenário recebe
 * isolado (uma instância nova por cenário). Os steps leem/gravam aqui
 * em vez de usar variáveis de módulo, o que evita vazamento de estado
 * entre cenários e permite rodá-los em paralelo com segurança.
 */
export class BusinessRulesWorld extends CucumberWorld {
  /** Resultado bruto da última ação executada por um step "When". */
  result: unknown;

  /** Erro capturado, quando o step "When" espera uma falha controlada. */
  error: unknown;

  /** Espaço livre para dados intermediários específicos do cenário. */
  data: Record<string, unknown> = {};

  reset(): void {
    this.result = undefined;
    this.error = undefined;
    this.data = {};
  }
}

setWorldConstructor(BusinessRulesWorld);
