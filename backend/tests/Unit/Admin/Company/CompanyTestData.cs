using EmpregaNet.Application.Admin.Company.Commands;
using EmpregaNet.Domain.Entities;
using EmpregaNet.Domain.Enums;

namespace EmpregaNet.Tests.Unit.Application.Admin.CompanyHandlers;

internal static class CompanyTestData
{
    internal static Address ValidAddress() =>
        new()
        {
            Street = "Av Paulista",
            Number = "1000",
            Complement = null,
            Neighborhood = "Bela Vista",
            City = "São Paulo",
            State = UF.SP,
            ZipCode = "01310100"
        };

    internal static CreateCompanyCommand ValidCreateCommand() =>
        new(
            CompanyName: "Empresa Teste Ltda",
            Cnpj: "11.222.333/0001-81",
            Email: "contato@empresa-teste.local",
            Phone: "1133334444",
            TypeOfActivity: nameof(TypeOfActivityEnum.Industry),
            Address: ValidAddress());

    internal static UpdateCompanyCommand ValidUpdateCommand() =>
        new(
            CompanyName: "Empresa Atualizada Ltda",
            Cnpj: "11.222.333/0001-81",
            Email: "novo@empresa-teste.local",
            Phone: "1144445555",
            TypeOfActivity: nameof(TypeOfActivityEnum.services),
            Address: ValidAddress());
}
