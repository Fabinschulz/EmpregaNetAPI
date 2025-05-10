using System.ComponentModel;

namespace EmpregaNet.Domain.Enums
{
    public enum DomainErrorEnum
    {
        /// <summary>
        /// Registro não encontrado
        /// </summary>
        [Description("Registro não encontrado")] RESOURCE_ID_NOT_FOUND,

        /// <summary>
        /// Erro ao processar a requisição
        /// </summary>
        [Description("Erro ao processar a requisição")] RESOURCE_ERROR,

        /// <summary>
        /// Usuário não encontrado
        /// </summary>
        [Description("Usuário não encontrado")] USER_NOT_FOUND,

        /// <summary>
        /// Usuário não possui permissão
        /// </summary>
        [Description("Usuário não possui permissão")] MISSING_RESOURCE_PERMISSION,

        /// <summary>
        /// Usuário possui permissão ou registro não existe
        /// </summary>
        [Description("Usuário não possui permissão ou registro não existe")] RECORD_NOT_EXISTS_OR_MISSING_PERMISSION,

        /// <summary>
        /// Ação inválida devido status do registro
        /// </summary>
        [Description("Ação inválida devido o status do registro")] INVALID_ACTION_FOR_STATUS,

        /// <summary>
        /// Ação inválida devido característica do registro
        /// </summary>
        [Description("Ação inválida devido característica do registro")] INVALID_ACTION_FOR_RECORD,

        /// <summary>
        /// Limite de requisições excedido
        /// </summary>
        [Description("Limite de requisições excedido")] TOO_MANY_REQUESTS,


        /// <summary>
        /// Falha ao gerar arquivo
        /// </summary>
        [Description("Falha ao gerar arquivo")] FILE_NOT_CREATED,

        /// <summary>
        /// Arquivo não encontrado
        /// </summary>
        [Description("Arquivo não encontrado")] FILE_NOT_FOUND,

        /// <summary>
        /// Arquivo já existe
        /// </summary>
        [Description("Arquivo já existe")] FILE_ALREADY_EXISTS,

        /// <summary>
        /// Não é permitido criar permissões duplicadas
        /// </summary>
        [Description("Permissões duplicadas")] DUPLICATED_PERMISSION_ENTRY,

        /// <summary>
        /// Dados do formulário inválidos
        /// </summary>
        [Description("Dados do formulário inválidos")] INVALID_FORM,

        /// <summary>
        /// Validação de regra de negócios
        /// </summary>
        [Description("Validação de regra de negócios")] INVALID_BUSINESS_RULE,

        /// <summary>
        /// Dados da solicitação inválidos
        /// </summary>
        [Description("Dados inválidos")] INVALID_QUERY_PARAMS,

        /// <summary>
        /// Registro já excluído
        /// </summary>
        [Description("Registro já excluído")] RECORD_IS_DELETED,

        /// <summary>
        /// Erro inesperado
        /// </summary>
        [Description("Erro inesperado. Tente novamente mais tarde.")] UNEXPECTED_EXCEPTION,

        /// <summary>
        /// Exclusão de documentos inválida
        /// </summary>
        [Description("Exclusão de documentos inválida")] INVALID_DOCUMENT_DELETE,

        /// <summary>
        /// Erro na integração de dados com a cobrança
        /// </summary>
        [Description("Erro na integração de dados com a cobrança")]
        BILLING_DATA_INTEGRATION_ERROR,

        /// <summary>
        /// Erro na esteira de processamento
        /// </summary>
        [Description("Erro na esteira de processamento")]
        WORKFLOW_ERROR,

        /// <summary>
        /// Documento obrigatório não enviado
        /// </summary>
        [Description("Documento obrigatório não enviado")]
        MISSING_REQUIRED_DOCUMENT,

        /// <summary>
        /// Token inválido (MFA)
        /// </summary>
        [Description("Token inválido (MFA)")]
        INVALID_MFA,

        /// <summary>
        /// Usuário e/ou senha inválidos
        /// </summary>
        [Description("Usuário e/ou senha inválidos")]
        INVALID_PASSWORD,

        /// <summary>
        /// Dados inválidos da Pessoa
        /// </summary>
        [Description("Dados inválidos da pessoa")]
        INVALID_PERSON_DATA,

        /// <summary>
        /// Filtro inválido
        /// </summary>
        [Description("Filtro inválido")]
        INVALID_QUERY_FILTER,

    }
}