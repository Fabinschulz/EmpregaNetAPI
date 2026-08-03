using EmpregaNet.Application.Admin.Company.Commands;
using EmpregaNet.Application.Common.Base;
using EmpregaNet.Application.Common.Exceptions;
using EmpregaNet.Domain.Entities;
using EmpregaNet.Domain.Enums;
using EmpregaNet.Domain.Interfaces;
using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;

namespace EmpregaNet.Tests.Unit.Application.Admin.CompanyHandlers;

public sealed class CreateCompanyCommandHandlerTests
{
    private readonly Mock<ICompanyRepository> _repo = new();

    [Fact]
    public async Task Handle_CnpjJaCadastrado_DeveLancarValidationAppException()
    {
        var entity = CompanyTestData.ValidCreateCommand();
        _repo.Setup(x => x.ExistsByCnpjAsync("11222333000181")).ReturnsAsync(true);
        var sut = new CreateCompanyCommandHandler(_repo.Object, NullLogger<CreateCompanyCommandHandler>.Instance);
        var cmd = new CreateCommand<CreateCompanyCommand>(entity);

        var act = async () => await sut.Handle(cmd, CancellationToken.None);

        await act.Should().ThrowAsync<ValidationAppException>()
            .Where(e => e.Code == DomainErrorEnum.RESOURCE_ALREADY_EXISTS);
        _repo.Verify(x => x.CreateAsync(It.IsAny<Company>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    /// <summary>
    /// O CNPJ alfanumérico tem letras na forma canónica. Normalizar a busca de duplicata com
    /// apenas-dígitos procuraria uma chave que nunca foi gravada, e a duplicata passaria.
    /// </summary>
    [Fact]
    public async Task Handle_CnpjAlfanumericoJaCadastrado_DeveLancarValidationAppException()
    {
        var entity = CompanyTestData.ValidCreateCommand() with { Cnpj = "12.ABC.345/01DE-35" };
        _repo.Setup(x => x.ExistsByCnpjAsync("12ABC34501DE35")).ReturnsAsync(true);
        var sut = new CreateCompanyCommandHandler(_repo.Object, NullLogger<CreateCompanyCommandHandler>.Instance);
        var cmd = new CreateCommand<CreateCompanyCommand>(entity);

        var act = async () => await sut.Handle(cmd, CancellationToken.None);

        await act.Should().ThrowAsync<ValidationAppException>()
            .Where(e => e.Code == DomainErrorEnum.RESOURCE_ALREADY_EXISTS);
        _repo.Verify(x => x.CreateAsync(It.IsAny<Company>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_CnpjNovo_DevePersistirERetornarId()
    {
        var entity = CompanyTestData.ValidCreateCommand();
        _repo.Setup(x => x.ExistsByCnpjAsync("11222333000181")).ReturnsAsync(false);
        _repo
            .Setup(x => x.CreateAsync(It.IsAny<Company>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Company c, CancellationToken _) =>
            {
                EntityIdHelper.SetCompanyId(c, 99L);
                return c;
            });
        var sut = new CreateCompanyCommandHandler(_repo.Object, NullLogger<CreateCompanyCommandHandler>.Instance);
        var cmd = new CreateCommand<CreateCompanyCommand>(entity);

        var id = await sut.Handle(cmd, CancellationToken.None);

        id.Should().Be(99L);
        _repo.Verify(
            x => x.CreateAsync(
                It.Is<Company>(c =>
                    c.RegistrationNumber == "11222333000181"
                    && c.CompanyName == entity.CompanyName
                    && c.Email == entity.Email
                    && c.TypeOfActivity == TypeOfActivityEnum.Industry),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }
}
