using EmpregaNet.Application.Companies.Command;
using EmpregaNet.Application.Utils.Helpers;
using EmpregaNet.Domain.Entities;
using EmpregaNet.Domain.Enums;

namespace EmpregaNet.Application.Companies.Factories;

public abstract record CompanyFactory
{

    /// <summary>
    /// Cria uma nova entidade Company sem associar vagas.
    /// </summary>
    public static Company Create(CreateCompanyCommand command)
    {
        var company = new Company
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

    /// <summary>
    /// Atualiza uma entidade Company existente, incluindo a sincronização de vagas.
    /// </summary>
    public static Company Update(Company company, UpdateCompanyCommand command)
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