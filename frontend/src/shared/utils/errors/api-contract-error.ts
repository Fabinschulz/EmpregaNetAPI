import { z } from 'zod';

export type ContractIssue = {
  path: string;
  message: string;
  code: string;
};

/**
 * A API respondeu, mas o corpo não corresponde ao schema de resposta.
 *
 * <para>É um defeito de contrato, não uma falha do utilizador nem da rede: ou o schema do cliente
 * está desatualizado, ou o endpoint mudou de forma. Tem tipo próprio para não se confundir com um
 * erro Axios (que tem status HTTP e corpo `DomainError`) nem com um erro genérico.</para>
 */
export class ApiContractError extends Error {
  readonly endpoint: string;
  readonly issues: readonly ContractIssue[];
  readonly zodError: z.ZodError;

  constructor(endpoint: string, zodError: z.ZodError) {
    super(`A resposta de ${endpoint} não corresponde ao contrato esperado.`);

    // `extends Error` perde a cadeia de protótipos quando o alvo de compilação é antigo, e um
    // `instanceof` a falhar em silêncio anularia toda a distinção que esta classe existe para fazer.
    Object.setPrototypeOf(this, ApiContractError.prototype);

    this.name = 'ApiContractError';
    this.endpoint = endpoint;
    this.zodError = zodError;
    this.issues = zodError.issues.map((issue) => ({
      path: issue.path.map(String).join('.'),
      message: issue.message,
      code: issue.code
    }));
  }
}

export function isApiContractError(err: unknown): err is ApiContractError {
  return err instanceof ApiContractError;
}

/** Um `ZodError` cru também é violação de contrato: os locais ainda não migrados lançam-no direto. */
export function isContractViolation(err: unknown): err is ApiContractError | z.ZodError {
  return isApiContractError(err) || err instanceof z.ZodError;
}
