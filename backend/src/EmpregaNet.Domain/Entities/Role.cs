using EmpregaNet.Domain.Interfaces;
using Microsoft.AspNetCore.Identity;

namespace EmpregaNet.Domain.Entities
{
    public class Role : IdentityRole<long>, IAggregateRoot
    {

        public DateTimeOffset DataInclusao { get; set; } = DateTimeOffset.Now;

        public DateTimeOffset? DataAlteracao { get; set; }

        public Role() : base() { }

    }
}