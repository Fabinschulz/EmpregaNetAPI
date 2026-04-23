using System.Reflection;
using EmpregaNet.Domain.Common;
using EmpregaNet.Domain.Entities;

namespace EmpregaNet.Tests.Unit.Application.Admin.CompanyHandlers;

internal static class EntityIdHelper
{
    internal static void SetCompanyId(Company company, long id)
    {
        var prop = typeof(BaseEntity).GetProperty(
            nameof(BaseEntity.Id),
            BindingFlags.Public | BindingFlags.Instance);
        prop!.SetValue(company, id);
    }
}
