using System.ComponentModel.DataAnnotations;
using EmpregaNet.Domain.Enums;
using EmpregaNet.Domain.Interfaces;

namespace EmpregaNet.Domain.Entities
{
    /// <summary>
    /// Entidade que representa um endereço
    /// Obs: Não é uma tabela no banco, mas um objeto de valor (Value Object).
    /// </summary>
    public class Address : IAggregateRoot
    {
        public required string Street { get; set; }

        public required string ZipCode { get; set; }

        public required string City { get; set; }

        [EnumDataType(typeof(UF))]
        public required UF State { get; set; }

        public required string Neighborhood { get; set; }

        public required string Number { get; set; }

        public string? Complement { get; set; }

    }
}