using System.ComponentModel;

namespace EmpregaNet.Domain.Enums
{
    /// <summary>
    /// Critério de ordenação do feed de vagas. O desempate é sempre por Id decrescente,
    /// para que a paginação continue estável entre requisições.
    /// </summary>
    public enum JobFeedSortEnum
    {
        /// <summary>Mais recentes primeiro (padrão).</summary>
        [Description("Mais recentes")] Recent,

        /// <summary>Maior faixa salarial primeiro; vagas sem salário divulgado vão para o fim.</summary>
        [Description("Maior salário")] Salary,

        /// <summary>Aderência ao termo buscado. Sem busca ativa, degrada para <see cref="Recent"/>.</summary>
        [Description("Relevância")] Relevance,

        /// <summary>Nome da empresa em ordem alfabética.</summary>
        [Description("Empresa")] Company,

        /// <summary>Estado e cidade em ordem alfabética.</summary>
        [Description("Localização")] Location
    }
}
