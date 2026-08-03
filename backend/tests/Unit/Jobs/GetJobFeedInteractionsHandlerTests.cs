using EmpregaNet.Application.Jobs.Queries;
using EmpregaNet.Domain.Interfaces;
using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;

namespace EmpregaNet.Tests.Unit.Application.Jobs;

public sealed class GetJobFeedInteractionsHandlerTests
{
    private const long CurrentUserId = 77;

    private readonly Mock<IJobApplicationRepository> _repo = new();
    private readonly Mock<IHttpCurrentUser> _currentUser = new();

    public GetJobFeedInteractionsHandlerTests()
    {
        _currentUser.SetupGet(x => x.UserId).Returns(CurrentUserId);
    }

    private GetJobFeedInteractionsHandler CreateSut() =>
        new(_repo.Object, _currentUser.Object, NullLogger<GetJobFeedInteractionsHandler>.Instance);

    [Fact]
    public async Task Handle_SemVagasInformadas_NaoDeveConsultarORepositorio()
    {
        var result = await CreateSut().Handle(new GetJobFeedInteractionsQuery([]), CancellationToken.None);

        result.AppliedJobIds.Should().BeEmpty();
        _repo.Verify(
            x => x.GetAppliedJobIdsAsync(It.IsAny<long>(), It.IsAny<IReadOnlyCollection<long>>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task Handle_DeveConsultarPeloUtilizadorDaSessaoEmUmaSoChamada()
    {
        _repo
            .Setup(x => x.GetAppliedJobIdsAsync(CurrentUserId, It.IsAny<IReadOnlyCollection<long>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync([1L, 7L]);

        var result = await CreateSut().Handle(
            new GetJobFeedInteractionsQuery([1L, 2L, 7L]),
            CancellationToken.None);

        result.AppliedJobIds.Should().BeEquivalentTo([1L, 7L]);

        // Uma chamada em lote - perguntar por cartão seria N+1 na rota mais acessada do produto.
        _repo.Verify(
            x => x.GetAppliedJobIdsAsync(CurrentUserId, It.IsAny<IReadOnlyCollection<long>>(), It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_SemCandidaturas_DeveDevolverListaVazia()
    {
        _repo
            .Setup(x => x.GetAppliedJobIdsAsync(It.IsAny<long>(), It.IsAny<IReadOnlyCollection<long>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync([]);

        var result = await CreateSut().Handle(
            new GetJobFeedInteractionsQuery([1L, 2L]),
            CancellationToken.None);

        result.AppliedJobIds.Should().BeEmpty();
    }

    [Fact]
    public void Validator_AcimaDoTetoDeIds_DeveFalhar()
    {
        var ids = Enumerable.Range(1, GetJobFeedInteractionsValidator.MaxJobIds + 1)
                            .Select(i => (long)i)
                            .ToArray();

        new GetJobFeedInteractionsValidator()
            .Validate(new GetJobFeedInteractionsQuery(ids))
            .IsValid.Should().BeFalse();
    }
}
