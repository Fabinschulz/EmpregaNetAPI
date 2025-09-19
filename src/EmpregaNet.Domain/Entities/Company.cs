using System.ComponentModel.DataAnnotations;
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
        [EnumDataType(typeof(TypeOfActivityEnum))]
        public TypeOfActivityEnum? TypeOfActivity { get; private set; }
        public string CompanyName { get; private set; }
        public Address Address { get; private set; }
        public string RegistrationNumber { get; private set; } // CNPJ
        public string Email { get; private set; }
        public string Phone { get; private set; }
        public ICollection<Job>? Jobs { get; private set; } = new List<Job>();

        public Company(string companyName, Address address, string registrationNumber, string email, string phone, TypeOfActivityEnum? typeOfActivity = null)
        {
            CompanyName = companyName;
            Address = address;
            RegistrationNumber = registrationNumber;
            Email = email;
            Phone = phone;
            TypeOfActivity = typeOfActivity;
        }

        public void UpdateDetails(string companyName, Address address, string registrationNumber, string email, string phone, TypeOfActivityEnum typeOfActivity)
        {
            this.CompanyName = companyName;
            this.Address = address;
            this.RegistrationNumber = registrationNumber;
            this.Email = email;
            this.Phone = phone;
            this.TypeOfActivity = typeOfActivity;
        }

        public void AddJob(Job newJob)
        {
            if (Jobs is null)
            {
                Jobs = new List<Job>();
            }
            Jobs.Add(newJob);
        }

        public void RemoveJob(Job jobToRemove)
        {
            if (jobToRemove is null)
                throw new ArgumentNullException(nameof(jobToRemove));

            Jobs?.Remove(jobToRemove);
        }

        public void ClearJobs()
        {
            Jobs?.Clear();
        }

    }
}