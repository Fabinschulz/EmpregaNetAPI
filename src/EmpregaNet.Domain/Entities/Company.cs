using EmpregaNet.Domain.Common;
using EmpregaNet.Domain.Enums;
using EmpregaNet.Domain.Interfaces;

namespace EmpregaNet.Domain.Entities
{
    /// <summary>
    /// Entidade que representa uma empresa que pode publicar várias vagas.
    /// </summary>
    public class Company : BaseEntity, IAggregateRoot
    {
        public required string CompanyName { get; set; }
        public required string RegistrationNumber { get; set; }
        public required string Email { get; set; }
        public required string Phone { get; set; }
        public required TypeOfActivityEnum TypeOfActivity { get; set; }
        public required Address Address { get; set; }

        public Company() { }

        public void UpdateCompany(
            string companyName,
            string email,
            string phone,
            TypeOfActivityEnum typeOfActivity,
            Address address)
        {
            CompanyName = companyName;
            Email = email;
            Phone = phone;
            TypeOfActivity = typeOfActivity;
            Address = address;
        }
    }
}