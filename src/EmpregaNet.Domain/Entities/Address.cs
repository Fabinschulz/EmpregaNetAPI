using System.ComponentModel.DataAnnotations;
using EmpregaNet.Domain.Enums;
using EmpregaNet.Domain.Interface;

namespace EmpregaNet.Domain.Entities
{
    /// <summary>
    /// Entidade que representa um endereço
    /// </summary>
    public class Address : IAggregateRoot
    {
        [Key]
        public long Id { get; set; }
        public required string Street { get; set; }

        public required string ZipCode { get; set; }

        public required string City { get; set; }

        [EnumDataType(typeof(UF))]
        public required UF State { get; set; }

        public required string District { get; set; } // Bairro

        public required string Number { get; set; }

        public string? Complement { get; set; }

    }
}