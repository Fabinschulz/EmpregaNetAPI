using EmpregaNet.Domain.Entities;
using EmpregaNet.Domain.Interfaces;
using EmpregaNet.Infra.Persistence.Database;

namespace EmpregaNet.Infra.Persistence.Repositories;

public class CompanyRepository : BaseRepository<Company>, ICompanyRepository
{
    public CompanyRepository(PostgreSqlContext context) : base(context)
    {
    }

}
