using EmpregaNet.Domain.Enums;
using EmpregaNet.Domain.Interfaces;
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
        public DateTimeOffset? BirthDate { get; set; }
        public ICollection<JobApplication> Applications { get; set; } = new List<JobApplication>();
        public CivilStatusEnum CivilStatus { get; set; }
        public GenderEnum? Gender { get; set; }
        public UserTypeEnum UserType { get; set; } = UserTypeEnum.Candidate; // converter para Roles depois
        public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
        public DateTimeOffset? UpdatedAt { get; set; } = null;
        public DateTimeOffset? DeletedAt { get; set; } = null;
        public bool IsDeleted { get; set; } = false;
        public string? ProfilePicture { get; set; }

        public User()
        {
            
        }
    }
}