using EmpregaNet.Application.Admin.Company.Commands.Delete;
using EmpregaNet.Application.Admin.Company.ViewModel;
using EmpregaNet.Application.Common.Base;
using EmpregaNet.Domain.Interfaces;
using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;

namespace EmpregaNet.Tests.Unit.Application.Admin.CompanyHandlers;

public sealed class DeleteCompanyHandlerTests
{
    private readonly Mock<ICompanyRepository> _repo = new();

    [Fact]
    public async Task Handle_RepositorioRemoveComSucesso_DeveRetornarTrue()
    {
        _repo.Setup(x => x.DeleteAsync(3L, It.IsAny<CancellationToken>())).ReturnsAsync(true);
        var sut = new DeleteCompanyHandler(_repo.Object, NullLogger<DeleteCompanyHandler>.Instance);

        var ok = await sut.Handle(new DeleteCommand<CompanyViewModel>(3), CancellationToken.None);

        ok.Should().BeTrue();
        _repo.Verify(x => x.DeleteAsync(3L, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_RepositorioLancaKeyNotFound_DevePropagarMensagemAmigavel()
    {
        _repo
            .Setup(x => x.DeleteAsync(9L, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new KeyNotFoundException("id"));
        var sut = new DeleteCompanyHandler(_repo.Object, NullLogger<DeleteCompanyHandler>.Instance);

        var act = async () => await sut.Handle(new DeleteCommand<CompanyViewModel>(9), CancellationToken.None);

        await act.Should().ThrowAsync<KeyNotFoundException>()
            .WithMessage("A empresa que você está tentando remover não existe ou já foi removida.");
    }
}
