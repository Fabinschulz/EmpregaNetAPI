using EmpregaNet.Application.Services;
using EmpregaNet.Domain.Entities;
using EmpregaNet.Domain.Interfaces;

namespace EmpregaNet.Domain.Services
{
    public class CompanyService : BaseService<Company>
    {
        private readonly ICompanyRepository _companyRepository;

        public CompanyService(ICompanyRepository repository) : base(repository)
        {
            _companyRepository = repository;
        }

    }
}