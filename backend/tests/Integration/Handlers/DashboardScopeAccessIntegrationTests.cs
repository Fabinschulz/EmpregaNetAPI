using EmpregaNet.Application.Auth;
using EmpregaNet.Application.Auth.ViewModel;
using EmpregaNet.Application.Common.Exceptions;
using EmpregaNet.Application.Dashboard.UseCase;
using EmpregaNet.Domain.Entities;
using EmpregaNet.Domain.Enums;
using EmpregaNet.Infra.Persistence.Database;
using EmpregaNet.Tests.Support;
using FluentAssertions;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Moq;

namespace EmpregaNet.Tests.Integration.Handlers;

/// <summary>
/// Fronteira de visibilidade do dashboard: quem vê os números de quem.
/// </summary>
/// <remarks>
/// <para>Integração e não unitário porque a decisão depende do Identity real: os papéis vêm de
/// <c>UserManager.GetRolesAsync</c> e o vínculo de <c>User.EmployerCompanyId</c> persistido. Um
/// <c>UserManager</c> falsificado testaria o duplo, não a regra.</para>
///
/// <para><b>Limitação do provider InMemory:</b> não reproduz constraints nem semântica do
/// PostgreSQL. O que se valida aqui é a decisão de autorização, não o SQL gerado.</para>
/// </remarks>
[Collection("Integration")]
public sealed class DashboardScopeAccessIntegrationTests : IDisposable
{
    private readonly InMemoryIdentityFixture _fx;

    public DashboardScopeAccessIntegrationTests(InMemoryIdentityFixture fx)
    {
        _fx = fx;
        _fx.ResetMocks();
    }

    public void Dispose()
    {
        _fx.ResetMocks();
        _fx.HttpUser.Reset();
    }

    [Fact]
    public async Task ResolveAsync_Admin_SemEmpresaPedida_DeveVerAPlataformaInteira()
    {
        var userId = await CreateUserAsync("admin_plat", RecruitmentRoleNames.Admin);
        SetupCurrentUser(userId, RecruitmentRoleNames.Admin);

        var scope = await ResolveAsync(null);

        scope.Scope.IsPlatformWide.Should().BeTrue();
        scope.Scope.CompanyId.Should().BeNull();
        scope.CompanyName.Should().BeNull();
    }

    [Fact]
    public async Task ResolveAsync_Admin_ComEmpresaPedida_DeveRestringirAquelaEmpresa()
    {
        var company = await CreateCompanyAsync("Acme Indústria");
        var userId = await CreateUserAsync("admin_company", RecruitmentRoleNames.Admin);
        SetupCurrentUser(userId, RecruitmentRoleNames.Admin);

        var scope = await ResolveAsync(company.Id);

        scope.Scope.CompanyId.Should().Be(company.Id);
        scope.CompanyName.Should().Be(company.Name);
    }

    [Fact]
    public async Task ResolveAsync_Admin_ComEmpresaInexistente_DeveFalharEmVezDeDevolverZeros()
    {
        var userId = await CreateUserAsync("admin_ghost", RecruitmentRoleNames.Admin);
        SetupCurrentUser(userId, RecruitmentRoleNames.Admin);

        // Um id inexistente aceito em silêncio produziria um painel inteiro de zeros,
        // indistinguível de uma empresa real sem movimento.
        var act = async () => await ResolveAsync(999_999);

        await act.Should().ThrowAsync<ValidationAppException>()
            .Where(e => e.Code == DomainErrorEnum.RESOURCE_ID_NOT_FOUND);
    }

    [Fact]
    public async Task ResolveAsync_Recrutador_DeveReceberSempreAPropriaEmpresa()
    {
        var company = await CreateCompanyAsync("Metalúrgica Sul");
        var userId = await CreateUserAsync("rec_own", RecruitmentRoleNames.Recruiter, company.Id);
        SetupCurrentUser(userId, RecruitmentRoleNames.Recruiter);

        var scope = await ResolveAsync(null);

        scope.Scope.IsPlatformWide.Should().BeFalse();
        scope.Scope.CompanyId.Should().Be(company.Id);
    }

    [Fact]
    public async Task ResolveAsync_Recrutador_PedindoOutraEmpresa_DeveSerNegado()
    {
        var ownCompany = await CreateCompanyAsync("Transportadora Norte");
        var otherCompany = await CreateCompanyAsync("Concorrente Ltda");
        var userId = await CreateUserAsync("rec_other", RecruitmentRoleNames.Recruiter, ownCompany.Id);
        SetupCurrentUser(userId, RecruitmentRoleNames.Recruiter);

        // Recusar, e não ignorar o parâmetro: ignorar devolveria dados válidos para a pergunta
        // errada, o que é pior do que um erro explícito.
        var act = async () => await ResolveAsync(otherCompany.Id);

        await act.Should().ThrowAsync<ValidationAppException>()
            .Where(e => e.Code == DomainErrorEnum.MISSING_RESOURCE_PERMISSION);
    }

    [Fact]
    public async Task ResolveAsync_GestorSemEmpresaVinculada_DeveExplicarOQueFalta()
    {
        var userId = await CreateUserAsync("mgr_unlinked", RecruitmentRoleNames.Manager);
        SetupCurrentUser(userId, RecruitmentRoleNames.Manager);

        var act = async () => await ResolveAsync(null);

        await act.Should().ThrowAsync<ValidationAppException>()
            .Where(e => e.Code == DomainErrorEnum.MISSING_RESOURCE_PERMISSION);
    }

    [Fact]
    public async Task ResolveAsync_Candidato_NaoDeveTerAcessoAsMetricas()
    {
        var userId = await CreateUserAsync("cand_metrics", "Candidate");
        SetupCurrentUser(userId, "Candidate");

        // Defesa em profundidade: a política da rota já bloqueia, mas o caso de uso não depende
        // disso para recusar.
        var act = async () => await ResolveAsync(null);

        await act.Should().ThrowAsync<ValidationAppException>()
            .Where(e => e.Code == DomainErrorEnum.MISSING_RESOURCE_PERMISSION);
    }

    private async Task<DashboardResolvedScope> ResolveAsync(long? companyId)
    {
        await using var scope = _fx.Services.CreateAsyncScope();
        var sut = scope.ServiceProvider.GetRequiredService<IDashboardScopeAccess>();
        return await sut.ResolveAsync(companyId, CancellationToken.None);
    }

    private void SetupCurrentUser(long userId, string role)
    {
        _fx.HttpUser.Setup(x => x.UserId).Returns(userId);
        _fx.HttpUser.Setup(x => x.GetContextUser()).Returns(new UserLoggedViewModel
        {
            AccessToken = "token",
            ExpiresIn = 3600,
            UserToken = new UserToken
            {
                Id = userId,
                Username = $"user_{userId}",
                Email = $"user_{userId}@test.local",
                Roles = [role],
                Claims = []
            }
        });
    }

    private async Task<long> CreateUserAsync(string prefix, string role, long? employerCompanyId = null)
    {
        var email = TestDataFactory.UniqueEmail(prefix);
        var userId = await AuthIntegrationTestHelper.RegisterConfirmedUserAsync(_fx.Services, email, prefix);

        using var scope = _fx.Services.CreateScope();
        var roles = scope.ServiceProvider.GetRequiredService<RoleManager<Role>>();
        if (!await roles.RoleExistsAsync(role))
        {
            await roles.CreateAsync(new Role { Name = role, DataInclusao = DateTimeOffset.UtcNow });
        }

        var users = scope.ServiceProvider.GetRequiredService<UserManager<User>>();
        var user = await users.FindByIdAsync(userId.ToString());
        user.Should().NotBeNull();

        await users.AddToRoleAsync(user!, role);

        if (employerCompanyId is { } companyId)
        {
            user!.EmployerCompanyId = companyId;
            await users.UpdateAsync(user);
        }

        return userId;
    }

    private async Task<(long Id, string Name)> CreateCompanyAsync(string name)
    {
        using var scope = _fx.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<PostgreSqlContext>();

        var company = new Company
        {
            CompanyName = $"{name} {Guid.NewGuid():N}",
            RegistrationNumber = Random.Shared.NextInt64(10_000_000_000_000, 99_999_999_999_999).ToString(),
            Email = TestDataFactory.UniqueEmail("company"),
            Phone = TestDataFactory.UniqueBrazilianCell(),
            TypeOfActivity = TypeOfActivityEnum.Industry,
            Address = new Address
            {
                Street = "Rua Teste",
                Number = "100",
                Neighborhood = "Centro",
                City = "Extrema",
                State = UF.MG,
                ZipCode = "37640-000"
            }
        };

        context.Companies.Add(company);
        await context.SaveChangesAsync();

        return (company.Id, company.CompanyName);
    }
}
