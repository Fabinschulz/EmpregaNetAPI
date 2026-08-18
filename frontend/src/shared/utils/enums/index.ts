export enum ApiStatusCode {
  Success = 200, // Sucesso ao realizar a requisição
  BadRequest = 400, // Requisição inválida
  Unauthorized = 401, // Usuário não autorizado
  Forbidden = 403, // Usuário não tem permissão para acessar este recurso
  NotFound = 404, // Recurso não encontrado
  Conflict = 409, // Conflito com o estado atual do recurso
  UnprocessableEntity = 422, // Corpo bem formado, mas semanticamente inválido
  TooManyRequests = 429, // Excesso de tentativas em pouco tempo
  InternalServerError = 500 // Erro interno do servidor
}

/** Primeiro status da faixa 4xx (erro atribuível ao pedido). */
export const CLIENT_ERROR_THRESHOLD: number = ApiStatusCode.BadRequest;

/** Primeiro status da faixa 5xx (falha atribuível ao servidor). */
export const SERVER_ERROR_THRESHOLD: number = ApiStatusCode.InternalServerError;

export function isClientError(status: number): boolean {
  return status >= CLIENT_ERROR_THRESHOLD && status < SERVER_ERROR_THRESHOLD;
}

export function isServerError(status: number): boolean {
  return status >= SERVER_ERROR_THRESHOLD;
}
