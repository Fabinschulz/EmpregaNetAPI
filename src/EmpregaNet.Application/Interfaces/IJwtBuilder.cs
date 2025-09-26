using EmpregaNet.Application.Auth.ViewModel;
using EmpregaNet.Domain.Entities;

namespace EmpregaNet.Application.Interfaces;

public interface IJwtBuilder
{
    Task<UserLoggedViewModel> BuildUserTokenAsync(User user);
    Task<List<UserPermissionVieModel>?> GetAllPermissions(string key);
}