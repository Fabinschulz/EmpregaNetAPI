using EmpregaNet.Domain.Interface;
using Microsoft.AspNetCore.Identity;

namespace OAuth.Domain.Entities
{
    public class Role : IdentityRole<long>, IAggregateRoot
    {

        public DateTime DataInclusao { get; set; } = DateTime.Now;

        public DateTime? DataAlteracao { get; set; }

        public Role() : base() { }

    }
}