using EmpregaNet.Domain.Enums;

namespace EmpregaNet.Application.User;

public class UserRegistry
{
    public required string Username { get; set; }
    public required string Email { get; set; }
    public required string Password { get; set; }
    public required string PasswordConfirmation { get; set; }
}

public class UserLoggedViewModel
{
    public required string AccessToken { get; set; }
    public required double ExpiresIn { get; set; }
    public required UserToken UserToken { get; set; }
    public required List<UserPermissionVieModel>? Permissions { get; set; }
    public string Key { get; set; } = string.Empty;

    public UserLoggedViewModel()
    {
        
    }
}

public class UserToken
{
    public required long Id { get; set; }
    public required string Username { get; set; }
    public required string Email { get; set; }
    public required IEnumerable<UserClaim> Claims { get; set; }   
}

public class UserClaim
{
    public required string Value { get; set; }
    public required string Type { get; set; }
}

public class UserPermissionVieModel
{
    public PermissionResourceEnum Resource { get; set; }
    public PermissionTypeEnum Type { get; set; }
}
