using EmpregaNet.Domain.Enums;
using EmpregaNet.Domain.Interface;
using Microsoft.AspNetCore.Identity;

namespace EmpregaNet.Domain.Entities
{
    /// <summary>
    /// Entidade que representa um usuário do sistema (candidato ou recrutador).
    /// </summary>
    /// <remarks>
    /// Herda de IdentityUser para integração com o ASP.NET Core Identity.
    /// </remarks>
    public class User : IdentityUser<long>, IAggregateRoot
    {
        public Address? Address { get; set; }
        public DateTime? BirthDate { get; set; }
        public ICollection<JobApplication> Applications { get; set; } = new List<JobApplication>();
        public CivilStatus CivilStatus { get; set; }
        public Gender? Gender { get; set; }
        public UserType UserType { get; set; } = UserType.Candidate; // converter para Roles depois
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; } = null;
        public DateTime? DeletedAt { get; set; } = null;
        public bool IsDeleted { get; set; } = false;
        public string? ProfilePicture { get; set; }
    }
}