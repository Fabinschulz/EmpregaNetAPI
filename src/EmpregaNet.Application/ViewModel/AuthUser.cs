using EmpregaNet.Domain.Enums;

namespace EmpregaNet.Application.ViewModel;

public class UserRegistry
{
    public required string Username { get; set; }
    public required string Email { get; set; }
    public required string Password { get; set; }
    public required string PasswordConfirmation { get; set; }
}

public class LoginVieModel
{
    public required string AccessToken { get; set; }
    public required double ExpiresIn { get; set; }
    public required  UserToken UserToken { get; set; }
    public required List<UserPermissionVieModel>? Permissions { get; set; }
}

public class UserToken
{
    public required long Id { get; set; }
    public required string User { get; set; }
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
