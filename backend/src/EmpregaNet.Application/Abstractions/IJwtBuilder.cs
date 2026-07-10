using EmpregaNet.Application.Auth.ViewModel;
using EmpregaNet.Domain.Entities;

namespace EmpregaNet.Application.Abstraction;

public interface IJwtBuilder
{
    Task<UserLoggedViewModel> BuildUserTokenAsync(User user);
}