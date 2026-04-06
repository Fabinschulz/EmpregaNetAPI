using EmpregaNet.Domain.Entities;
using EmpregaNet.Domain.Enums;

namespace EmpregaNet.Application.Users.ViewModel;

public sealed class UserViewModel
{
    public long Id { get; set; }
    public string Username { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? PhoneNumber { get; set; }
    public UserTypeEnum UserType { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
}

public static class UserMapper
{
    public static UserViewModel ToViewModel(this User user)
    {
        return new UserViewModel
        {
            Id = user.Id,
            Username = user.UserName ?? string.Empty,
            Email = user.Email ?? string.Empty,
            PhoneNumber = user.PhoneNumber,
            UserType = user.UserType,
            CreatedAt = user.CreatedAt
        };
    }
}
