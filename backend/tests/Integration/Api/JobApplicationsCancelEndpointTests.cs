using System.Text.Json;
using EmpregaNet.Api.Middleware;
using EmpregaNet.Application.Auth.ViewModel;
using EmpregaNet.Application.JobApplications.Commands;
using EmpregaNet.Domain.Common;
using EmpregaNet.Domain.Entities;
using EmpregaNet.Domain.Enums;
using EmpregaNet.Infra.Persistence.Database;
using EmpregaNet.Infra.Persistence.Repositories;
using EmpregaNet.Tests.Support;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;

namespace EmpregaNet.Tests.Integration.Api;

/// <summary>
/// Contrato do <c>PUT /api/jobapplications/{id}/cancel</c>: o comando corre sobre o repositório e o
/// EF reais, e a excepção que ele produz atravessa o <see cref="GlobalExceptionHandler"/> — que é
/// quem decide o código HTTP e o corpo de erro devolvidos ao cliente.
/// </summary>
/// <remarks>
/// Não sobe servidor HTTP: o projecto não tem <c>WebApplicationFactory</c> e o arranque real exige
/// PostgreSQL, Redis e JWT. O que fica de fora é o roteamento MVC e o <c>[Authorize]</c> da classe;
/// tudo o que decide <b>o código de estado</b> está exercitado aqui.
///
/// Limitação do provider InMemory: não reproduz constraints nem semântica do PostgreSQL.
/// </remarks>
[Collection("Integration")]
public sealed class JobApplicationsCancelEndpointTests
{
    private static readonly JsonSerializerOptions ReadOptions = new(JsonSerializerDefaults.Web);

    private readonly InMemoryIdentityFixture _fixture;

    public JobApplicationsCancelEndpointTests(InMemoryIdentityFixture fixture)
    {
        _fixture = fixture;
    }

    private static UserLoggedViewModel CandidateContext(long userId) => new()
    {
        AccessToken = "token",
        ExpiresIn = 3600,
        UserToken = new UserToken
        {
            Id = userId,
            Username = "candidato",
            Email = "candidato@test.local",
            Roles = ["Candidate"],
            Claims = []
        }
    };

    private async Task<(long applicationId, IServiceScope scope)> SeedApplicationAsync(
        long userId,
        ApplicationStatusEnum status)
    {
        var scope = _fixture.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<PostgreSqlContext>();
        var repository = new JobApplicationRepository(context);

        var application = new JobApplication(jobId: Random.Shared.Next(1000, 9999), userId: userId);
        await repository.CreateAsync(application, CancellationToken.None);

        if (status != ApplicationStatusEnum.Pending)
        {
            application.ChangeStatus(status);
            await repository.UpdateAsync(application, CancellationToken.None);
        }

        return (application.Id, scope);
    }

    private static CancelJobApplicationHandler CreateHandler(IServiceScope scope, long currentUserId)
    {
        var context = scope.ServiceProvider.GetRequiredService<PostgreSqlContext>();

        var currentUser = new Mock<IHttpCurrentUser>();
        currentUser.SetupGet(x => x.UserId).Returns(currentUserId);
        currentUser.Setup(x => x.GetContextUser()).Returns(CandidateContext(currentUserId));

        return new CancelJobApplicationHandler(
            new JobApplicationRepository(context),
            currentUser.Object,
            NullLogger<CancelJobApplicationHandler>.Instance);
    }

    /// <summary>Faz a excepção percorrer o mesmo caminho de resposta que percorreria na API.</summary>
    private static async Task<DomainError> ToHttpErrorAsync(Func<Task> act)
    {
        Exception? captured = null;
        try
        {
            await act();
        }
        catch (Exception ex)
        {
            captured = ex;
        }

        captured.Should().NotBeNull("a operação devia ter sido recusada");

        var handler = new GlobalExceptionHandler(NullLogger<GlobalExceptionHandler>.Instance);
        var httpContext = new DefaultHttpContext();
        httpContext.Items["Correlation-ID"] = "correlation-de-teste";
        using var body = new MemoryStream();
        httpContext.Response.Body = body;

        (await handler.TryHandleAsync(httpContext, captured!, CancellationToken.None)).Should().BeTrue();

        body.Position = 0;
        return (await JsonSerializer.DeserializeAsync<DomainError>(body, ReadOptions))!;
    }

    [Fact]
    public async Task Cancel_CandidaturaPropriaEmAnalise_DeveDevolver200EStatusCanceladoPeloCandidato()
    {
        const long candidateId = 9101;
        var (applicationId, scope) = await SeedApplicationAsync(candidateId, ApplicationStatusEnum.Processing);
        using (scope)
        {
            var result = await CreateHandler(scope, candidateId)
                .Handle(new CancelJobApplicationCommand(applicationId), CancellationToken.None);

            result.Id.Should().Be(applicationId);
            result.Status.Should().Be(ApplicationStatusEnum.CanceledByCandidate);
        }

        // Leitura subsequente, noutro escopo: o cancelamento ficou persistido.
        using var readScope = _fixture.Services.CreateScope();
        var stored = await new JobApplicationRepository(readScope.ServiceProvider.GetRequiredService<PostgreSqlContext>())
            .GetByIdAsync(applicationId, CancellationToken.None);

        stored!.Status.Should().Be(ApplicationStatusEnum.CanceledByCandidate);
    }

    [Fact]
    public async Task Cancel_CandidaturaInexistente_DeveDevolver404()
    {
        using var scope = _fixture.Services.CreateScope();
        var handler = CreateHandler(scope, currentUserId: 9102);

        var error = await ToHttpErrorAsync(() =>
            handler.Handle(new CancelJobApplicationCommand(987654), CancellationToken.None));

        error.StatusCode.Should().Be(StatusCodes.Status404NotFound);
        error.Code.Should().Be(DomainErrorEnum.RESOURCE_ID_NOT_FOUND);
    }

    /// <summary>
    /// RBAC-1: a candidatura de outra pessoa devolve exactamente o mesmo <c>404</c> da inexistente.
    /// Um <c>403</c> aqui confirmaria que aquele id existe, e bastaria percorrer ids para mapear a
    /// base.
    /// </summary>
    [Fact]
    public async Task Cancel_CandidaturaDeOutroUtilizador_DeveDevolverOMesmo404DaInexistente()
    {
        const long ownerId = 9103;
        const long intruderId = 9104;
        var (applicationId, scope) = await SeedApplicationAsync(ownerId, ApplicationStatusEnum.Processing);

        using (scope)
        {
            var alheia = await ToHttpErrorAsync(() =>
                CreateHandler(scope, intruderId).Handle(new CancelJobApplicationCommand(applicationId), CancellationToken.None));

            var inexistente = await ToHttpErrorAsync(() =>
                CreateHandler(scope, intruderId).Handle(new CancelJobApplicationCommand(987655), CancellationToken.None));

            alheia.StatusCode.Should().Be(StatusCodes.Status404NotFound);
            alheia.StatusCode.Should().Be(inexistente.StatusCode);
            alheia.Code.Should().Be(inexistente.Code);
            alheia.Message.Should().Be(inexistente.Message);
        }

        using var readScope = _fixture.Services.CreateScope();
        var stored = await new JobApplicationRepository(readScope.ServiceProvider.GetRequiredService<PostgreSqlContext>())
            .GetByIdAsync(applicationId, CancellationToken.None);

        stored!.Status.Should().Be(ApplicationStatusEnum.Processing);
    }

    [Fact]
    public async Task Cancel_CandidaturaJaAprovada_DeveDevolver400ComCodigoDeStatusInvalido()
    {
        const long candidateId = 9105;
        var (applicationId, scope) = await SeedApplicationAsync(candidateId, ApplicationStatusEnum.Approved);

        using (scope)
        {
            var error = await ToHttpErrorAsync(() =>
                CreateHandler(scope, candidateId).Handle(new CancelJobApplicationCommand(applicationId), CancellationToken.None));

            error.StatusCode.Should().Be(StatusCodes.Status400BadRequest);
            error.Code.Should().Be(DomainErrorEnum.INVALID_ACTION_FOR_STATUS);
        }
    }

    /// <summary>
    /// CA-13: cancelar como empresa é o <c>PUT</c> de status já existente. Staff de recrutamento é
    /// recusado aqui mesmo sobre uma candidatura que existe e está em estado cancelável — a recusa
    /// vem do papel, não do estado nem da posse.
    /// </summary>
    [Theory]
    [InlineData("Admin")]
    [InlineData("Recruiter")]
    [InlineData("Manager")]
    public async Task Cancel_PerfilDeRecrutamento_DeveDevolver400ENaoAlterarACandidatura(string role)
    {
        const long candidateId = 9106;
        var (applicationId, scope) = await SeedApplicationAsync(candidateId, ApplicationStatusEnum.Processing);

        using (scope)
        {
            var context = scope.ServiceProvider.GetRequiredService<PostgreSqlContext>();

            var staff = new Mock<IHttpCurrentUser>();
            staff.SetupGet(x => x.UserId).Returns(candidateId);
            staff.Setup(x => x.GetContextUser()).Returns(new UserLoggedViewModel
            {
                AccessToken = "token",
                ExpiresIn = 3600,
                UserToken = new UserToken
                {
                    Id = candidateId,
                    Username = "recrutador",
                    Email = "recrutador@test.local",
                    Roles = [role],
                    Claims = []
                }
            });

            var handler = new CancelJobApplicationHandler(
                new JobApplicationRepository(context),
                staff.Object,
                NullLogger<CancelJobApplicationHandler>.Instance);

            var error = await ToHttpErrorAsync(() =>
                handler.Handle(new CancelJobApplicationCommand(applicationId), CancellationToken.None));

            error.StatusCode.Should().Be(StatusCodes.Status400BadRequest);
            error.Code.Should().Be(DomainErrorEnum.INVALID_ACTION_FOR_RECORD);
        }

        using var readScope = _fixture.Services.CreateScope();
        var stored = await new JobApplicationRepository(readScope.ServiceProvider.GetRequiredService<PostgreSqlContext>())
            .GetByIdAsync(applicationId, CancellationToken.None);

        stored!.Status.Should().Be(ApplicationStatusEnum.Processing);
    }
}
