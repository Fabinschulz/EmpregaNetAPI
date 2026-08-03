using System.ComponentModel.DataAnnotations;
using EmpregaNet.Domain.Enums;

namespace EmpregaNet.Domain.Entities
{
    /// <summary>
    /// Localização da vaga (objeto de valor - colunas na própria tabela <c>Jobs</c>, não tabela à parte).
    /// </summary>
    /// <remarks>
    /// Vive na vaga e não é derivada de <see cref="Company"/> por duas razões: a vaga pode ser em
    /// cidade diferente da sede, e o filtro geográfico do feed precisa de coluna indexável em
    /// <c>Jobs</c> e derivar da empresa tornaria o join obrigatório em toda consulta.
    ///
    /// Vaga remota mantém cidade/UF de referência (base da contratação); quem diz que é remota
    /// é <see cref="WorkModelEnum"/>.
    /// </remarks>
    public class JobLocation
    {

        [EnumDataType(typeof(UF))]
        public required UF State { get; set; }
        public string Country { get; set; } = "BR";
        public required string City { get; set; }
    }
}
