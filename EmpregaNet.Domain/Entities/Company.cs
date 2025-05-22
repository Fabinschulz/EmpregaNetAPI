using System.ComponentModel.DataAnnotations;
using EmpregaNet.Domain.Enums;
using EmpregaNet.Domain.Interface;

namespace EmpregaNet.Domain.Entities
{
    /// <summary>
    /// Entidade que representa uma empresa que pode publicar várias vagas.
    /// </summary>
    public class Company : BaseEntity, IAggregateRoot
    {
        [EnumDataType(typeof(TypeOfActivityEnum))]
        public TypeOfActivityEnum? TypeOfActivity { get; set; }
        public required string CompanyName { get; set; }
        public required Address Address { get; set; }
        public required string RegistrationNumber { get; set; } // CNPJ
        public required string Email { get; set; }
        public required string Phone { get; set; }
        public ICollection<Job> Jobs { get; set; } = new List<Job>();

    }

    public class CompanyCommand
    {
        public string? CompanyName { get; set; }
        public string? RegistrationNumber { get; set; }
        public string? Email { get; set; }
        public string? Phone { get; set; }
        public Address? Address { get; set; }
        public TypeOfActivityEnum? TypeOfActivity { get; set; }
        public ICollection<Job>? Jobs { get; set; }
    }
}