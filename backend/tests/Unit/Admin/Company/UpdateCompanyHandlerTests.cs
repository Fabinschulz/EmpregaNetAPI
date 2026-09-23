using EmpregaNet.Application.Abstraction;
using EmpregaNet.Application.Admin.Company.Commands;
using EmpregaNet.Application.Admin.Company.ViewModel;
using EmpregaNet.Application.Auth;
using EmpregaNet.Application.Auth.ViewModel;
using EmpregaNet.Application.Common.Base;
using EmpregaNet.Application.Common.Exceptions;
using EmpregaNet.Domain.Entities;
using EmpregaNet.Domain.Enums;
using EmpregaNet.Domain.Interfaces;
using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;

namespace EmpregaNet.Tests.Unit.Application.Admin.CompanyHandlers;

public sealed class UpdateCompanyHandlerTests
{
    private readonly Mock<ICompanyRepository> _repo = new();
    private readonly Mock<IHttpCurrentUser> _currentUser = new();

    public UpdateCompanyHandlerTests() => GivenAuthenticatedUser(RecruitmentRoleNames.Admin);

    private void GivenAuthenticatedUser(params string[] roles)
    {
        _currentUser.Setup(x => x.GetContextUser()).Returns(new UserLoggedViewModel
        {
            AccessToken = "token",
            ExpiresIn = 3600,
            UserToken = new UserToken
            {
                Id = 1,
                Username = "admin",
                Email = "admin@test.local",
                Roles = [.. roles],
                Claims = []
            }
        });
    }

    private UpdateCompanyHandler CreateSut() =>
        new(_repo.Object, _currentUser.Object, NullLogger<UpdateCompanyHandler>.Instance);

    [Fact]
    public async Task Handle_EmpresaInexistente_DeveLancarValidationAppException()
    {
        _repo.Setup(x => x.GetByIdAsync(5L)).ReturnsAsync((Company?)null);
        var sut = CreateSut();
        var cmd = new UpdateCommand<UpdateCompanyCommand, CompanyViewModel>(5, CompanyTestData.ValidUpdateCommand());

        var act = async () => await sut.Handle(cmd, CancellationToken.None);

        await act.Should().ThrowAsync<ValidationAppException>()
            .Where(e => e.Code == DomainErrorEnum.RESOURCE_ID_NOT_FOUND);
        _repo.Verify(x => x.UpdateAsync(It.IsAny<Company>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    // Defesa em profundidade: reforço na Application além do [Authorize(Policy = Administrador)] da API.
    [Theory]
    [InlineData(RecruitmentRoleNames.Recruiter)]
    [InlineData(RecruitmentRoleNames.Manager)]
    public async Task Handle_UsuarioNaoAdministrador_DeveRecusarAntesDeConsultarRepositorio(string role)
    {
        GivenAuthenticatedUser(role);
        var cmd = new UpdateCommand<UpdateCompanyCommand, CompanyViewModel>(5, CompanyTestData.ValidUpdateCommand());
        var sut = CreateSut();

        var act = async () => await sut.Handle(cmd, CancellationToken.None);

        var assertion = await act.Should().ThrowAsync<ValidationAppException>();
        assertion.Which.Code.Should().Be(DomainErrorEnum.MISSING_RESOURCE_PERMISSION);
        _repo.Verify(x => x.GetByIdAsync(It.IsAny<long>()), Times.Never);
    }

    [Fact]
    public async Task Handle_EmpresaExcluida_DeveLancarValidationAppException()
    {
        var company = new Company
        {
            CompanyName = "X",
            RegistrationNumber = "11222333000181",
            Email = "x@test.local",
            Phone = "1100000000",
            TypeOfActivity = TypeOfActivityEnum.Industry,
            Address = CompanyTestData.ValidAddress(),
            IsDeleted = true
        };
        EntityIdHelper.SetCompanyId(company, 5L);
        _repo.Setup(x => x.GetByIdAsync(5L)).ReturnsAsync(company);
        var sut = CreateSut();
        var cmd = new UpdateCommand<UpdateCompanyCommand, CompanyViewModel>(5, CompanyTestData.ValidUpdateCommand());

        var act = async () => await sut.Handle(cmd, CancellationToken.None);

        await act.Should().ThrowAsync<ValidationAppException>()
            .Where(e => e.Code == DomainErrorEnum.INVALID_ACTION_FOR_STATUS);
    }

    [Fact]
    public async Task Handle_DadosValidos_DeveAtualizarERetornarViewModel()
    {
        var company = new Company
        {
            CompanyName = "Nome Antigo",
            RegistrationNumber = "11222333000181",
            Email = "antigo@test.local",
            Phone = "1100000000",
            TypeOfActivity = TypeOfActivityEnum.Industry,
            Address = CompanyTestData.ValidAddress()
        };
        EntityIdHelper.SetCompanyId(company, 12L);
        _repo.Setup(x => x.GetByIdAsync(12L)).ReturnsAsync(company);
        var sut = CreateSut();
        var update = CompanyTestData.ValidUpdateCommand();
        var cmd = new UpdateCommand<UpdateCompanyCommand, CompanyViewModel>(12, update);

        var vm = await sut.Handle(cmd, CancellationToken.None);

        vm.Id.Should().Be(12L);
        vm.CompanyName.Should().Be(update.CompanyName);
        vm.Email.Should().Be(update.Email);
        vm.Phone.Should().Be(update.Phone);
        vm.TypeOfActivity.Should().NotBeNullOrWhiteSpace();
        _repo.Verify(x => x.UpdateAsync(It.IsAny<Company>(), It.IsAny<CancellationToken>()), Times.Once);
    }
}
