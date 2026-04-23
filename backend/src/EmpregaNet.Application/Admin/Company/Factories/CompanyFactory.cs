using EmpregaNet.Application.Admin.Company.Commands;
using EmpregaNet.Application.Utils.Helpers;
using EmpregaNet.Domain.Enums;
using DomainCompany = EmpregaNet.Domain.Entities.Company;

namespace EmpregaNet.Application.Admin.Company.Factories;

public abstract record CompanyFactory
{
    public static DomainCompany Create(CreateCompanyCommand command)
    {
        var company = new DomainCompany
        {
            CompanyName = command.CompanyName,
            Address = command.Address,
            RegistrationNumber = command.Cnpj.OnlyNumbers().Trim(),
            Email = command.Email,
            Phone = command.Phone,
            TypeOfActivity = Enum.TryParse<TypeOfActivityEnum>(command.TypeOfActivity, out var typeOfActivity) ? typeOfActivity : TypeOfActivityEnum.NaoSelecionado
        };

        return company;
    }

    public static DomainCompany Update(DomainCompany company, UpdateCompanyCommand command)
    {
        company.UpdateCompany(
            companyName: command.CompanyName,
            address: command.Address,
            email: command.Email,
            phone: command.Phone,
            typeOfActivity: Enum.TryParse<TypeOfActivityEnum>(command.TypeOfActivity, out var typeOfActivity) ? typeOfActivity : TypeOfActivityEnum.NaoSelecionado
        );

        return company;
    }
}
