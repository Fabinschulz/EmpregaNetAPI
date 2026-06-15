export enum ApiStatusCode {
  Success = 200, // Sucesso ao realizar a requisição
  BadRequest = 400, // Requisição inválida
  Unauthorized = 401, // Usuário não autorizado
  Forbidden = 403, // Usuário não tem permissão para acessar este recurso
  NotFound = 404, // Recurso não encontrado
  InternalServerError = 500 // Erro interno do servidor
}
