/** Variáveis de mutation autenticada com payload. */
export type AuthMutationVars<TDto = unknown> = {
  dto: TDto;
};

/** Variáveis de mutation autenticada por id + payload */
export type AuthIdMutationVars<TDto = unknown> = {
  id: number;
  dto: TDto;
};

/** Variáveis de mutation autenticada apenas por id. */
export type AuthIdVars = {
  id: number;
};
