using EmpregaNet.Domain.Common;
using Microsoft.AspNetCore.Identity;

namespace EmpregaNet.Domain.Entities
{
    public class User : IdentityUser
    {
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; } = null;
        public DateTime? DeletedAt { get; set; } = null;
        public bool IsDeleted { get; set; } = false;
    }

    public class UserLogin
    {
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }
}