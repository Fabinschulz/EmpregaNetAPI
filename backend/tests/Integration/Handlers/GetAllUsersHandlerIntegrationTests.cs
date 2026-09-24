using EmpregaNet.Application.Auth;
using EmpregaNet.Application.Auth.ViewModel;
using EmpregaNet.Application.Common.Exceptions;
using EmpregaNet.Application.Users.Queries;
using EmpregaNet.Application.Users.ViewModel;
using EmpregaNet.Domain.Common;
using EmpregaNet.Domain.Entities;
using EmpregaNet.Domain.Enums;
using EmpregaNet.Tests.Support;
using FluentAssertions;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Moq;

namespace EmpregaNet.Tests.Integration.Handlers;

/// <summary>
/// <c>GET /api/admin?userType=</c> (tela <c>/admin/usuarios</c>): filtro por tipo de usuário combinado
/// com situação e busca, aplicado antes da contagem e da paginação, atrás da guarda de administrador.
/// </summary>
/// <remarks>
/// <para>Integração porque o handler lê <c>UserManager&lt;User&gt;.Users</c>; o fixture já regista o
/// Identity sobre o contexto InMemory (tarefa 2.1 da <c>emp-filtro-tipo-usuario-admin</c>: nenhum
/// ajuste de suporte foi necessário). Um <c>UserManager</c> falsificado testaria o duplo.</para>
///
/// <para>A tabela de usuários é partilhada por toda a coleção: cada cenário marca os seus usuários com
/// um termo único no nome e combina-o com o filtro quando precisa de contagem exata. O cenário do filtro
/// isolado não usa busca e por isso verifica pertença, não total.</para>
///
/// <para><b>Limitação do provider InMemory:</b> a execução do <c>WHERE</c> em Postgres real não é
/// provada aqui (fica para o E2E).</para>
/// </remarks>
[Collection("Integration")]
public sealed class GetAllUsersHandlerIntegrationTests
{
    private readonly InMemoryIdentityFixture _fixture;

    public GetAllUsersHandlerIntegrationTests(InMemoryIdentityFixture fixture)
    {
        _fixture = fixture;
    }

    // ---------- CA-02: tipo restringe a lista, combinado com busca e situação ----------

    [Fact]
    public async Task Handle_CA02_FiltroDeTipoIsolado_DeveDevolverSoUsuariosDoTipo()
    {
        // Arrange
        var tag = Token("isol");
        var recruiter = await CreateUserAsync(tag, UserTypeEnum.Recruiter);
        var candidate = await CreateUserAsync(tag, UserTypeEnum.Candidate);
        var manager = await CreateUserAsync(tag, UserTypeEnum.Manager);

        // Act: sem busca — o universo é a tabela inteira.
        var result = await HandleAsAdminAsync(Query(userType: "Recruiter", size: 10_000));

        // Assert
        // Leitura devolve a Description pt-BR (contrato de userType da API).
        result.Data.Should().OnlyContain(u => u.UserType == "Recrutador");
        var ids = result.Data.Select(u => u.Id).ToList();
        ids.Should().Contain(recruiter).And.NotContain(candidate).And.NotContain(manager);
        result.TotalItems.Should().Be(result.Data.Count);
    }

    [Fact]
    public async Task Handle_CA02_TipoComSituacao_DeveAplicarOsDois()
    {
        // Arrange
        var tag = Token("sit");
        var activeRecruiter = await CreateUserAsync(tag, UserTypeEnum.Recruiter);
        var deletedRecruiter = await CreateUserAsync(tag, UserTypeEnum.Recruiter, deleted: true);
        var deletedCandidate = await CreateUserAsync(tag, UserTypeEnum.Candidate, deleted: true);

        // Act: sem busca, universo inteiro; verifica pertença.
        var result = await HandleAsAdminAsync(Query(userType: "Recruiter", isDeleted: true, size: 10_000));

        // Assert
        var ids = result.Data.Select(u => u.Id).ToList();
        ids.Should().Contain(deletedRecruiter)
            .And.NotContain(activeRecruiter)
            .And.NotContain(deletedCandidate);
        result.Data.Should().OnlyContain(u => u.IsDeleted);
    }

    [Fact]
    public async Task Handle_CA02_TipoComBusca_DeveAplicarOsDois()
    {
        // Arrange
        var tag = Token("busca");
        var match = await CreateUserAsync(tag, UserTypeEnum.Manager);
        await CreateUserAsync(Token("outro"), UserTypeEnum.Manager); // mesmo tipo, busca não casa
        await CreateUserAsync(tag, UserTypeEnum.Candidate);          // busca casa, tipo errado

        // Act
        var result = await HandleAsAdminAsync(Query(userType: "Manager", search: tag));

        // Assert
        result.Data.Should().ContainSingle().Which.Id.Should().Be(match);
        result.TotalItems.Should().Be(1);
    }

    [Fact]
    public async Task Handle_CA02_TipoSituacaoEBuscaJuntos_DeveAplicarOsTres()
    {
        // Arrange: cada "distrator" falha em exatamente um dos três critérios.
        var tag = Token("tres");
        var expected = await CreateUserAsync(tag, UserTypeEnum.Recruiter, deleted: false);
        await CreateUserAsync(tag, UserTypeEnum.Recruiter, deleted: true);          // situação errada
        await CreateUserAsync(tag, UserTypeEnum.Admin, deleted: false);             // tipo errado
        await CreateUserAsync(Token("fora"), UserTypeEnum.Recruiter, deleted: false); // busca não casa

        // Act
        var result = await HandleAsAdminAsync(Query(userType: "Recruiter", isDeleted: false, search: tag));

        // Assert
        result.Data.Should().ContainSingle().Which.Id.Should().Be(expected);
        result.TotalItems.Should().Be(1);
    }

    // ---------- CA-05: contagem e paginação refletem só o filtro ----------

    [Fact]
    public async Task Handle_CA05_TotalItemsEPaginacao_DevemRefletirSoOTipoFiltrado()
    {
        // Arrange: 3 recrutadores e 2 candidatos com o mesmo termo.
        var tag = Token("pag");
        var recruiters = new List<long>();
        for (var i = 0; i < 3; i++)
        {
            recruiters.Add(await CreateUserAsync(tag, UserTypeEnum.Recruiter));
        }

        var candidates = new List<long>
        {
            await CreateUserAsync(tag, UserTypeEnum.Candidate),
            await CreateUserAsync(tag, UserTypeEnum.Candidate)
        };

        // Act
        var page1 = await HandleAsAdminAsync(Query(userType: "Recruiter", search: tag, page: 1, size: 2));
        var page2 = await HandleAsAdminAsync(Query(userType: "Recruiter", search: tag, page: 2, size: 2));

        // Assert
        page1.TotalItems.Should().Be(3);
        page2.TotalItems.Should().Be(3);
        page1.Data.Should().HaveCount(2);
        page2.Data.Should().ContainSingle();

        var seen = page1.Data.Concat(page2.Data).Select(u => u.Id).ToList();
        seen.Should().BeEquivalentTo(recruiters);
        seen.Should().NotIntersectWith(candidates);
    }

    // ---------- Contrato e segurança ----------

    [Theory]
    [InlineData("recruiter")]
    [InlineData("RECRUITER")]
    [InlineData("Recruiter")]
    [InlineData(" recruiter ")] // espaços nas pontas são ignorados
    public async Task Handle_Contrato_NomeDoEnumEmQualquerCaixa_DeveSerAceito(string userType)
    {
        // Arrange
        var tag = Token("caixa");
        var recruiter = await CreateUserAsync(tag, UserTypeEnum.Recruiter);
        await CreateUserAsync(tag, UserTypeEnum.Candidate);

        // Act
        var result = await HandleAsAdminAsync(Query(userType: userType, search: tag));

        // Assert
        result.Data.Should().ContainSingle().Which.Id.Should().Be(recruiter);
    }

    [Theory]
    [InlineData("Recrutador")]       // rótulo pt-BR: leitura usa Description, escrita usa o nome do enum
    [InlineData("NaoSelecionado")]
    [InlineData("naoselecionado")]
    [InlineData("0")]                // NaoSelecionado por número
    [InlineData("99")]               // TryParse aceita qualquer número; IsDefined recusa
    [InlineData("Desconhecido")]
    [InlineData("1")]                   // número de membro declarado (Candidate): só o nome é aceito
    [InlineData("Candidate,Recruiter")] // lista com vírgula: Enum.TryParse faria OU bit a bit (= Admin)
    public async Task Handle_Contrato_UserTypeInvalido_DeveLancarInvalidQueryFilter(string userType)
    {
        // Act
        var act = async () => await HandleAsAdminAsync(Query(userType: userType));

        // Assert
        (await act.Should().ThrowAsync<ValidationAppException>())
            .Which.Code.Should().Be(DomainErrorEnum.INVALID_QUERY_FILTER);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("Recruiter")]
    [InlineData("99")] // a guarda vem antes do filtro: mesmo com valor inválido, a resposta é de permissão
    public async Task Handle_Seguranca_NaoAdmin_DeveSerBarradoComOuSemUserType(string? userType)
    {
        // Arrange
        var currentUser = CurrentUserWithRole(RecruitmentRoleNames.Recruiter);

        // Act
        var act = async () => await HandleAsync(currentUser, Query(userType: userType));

        // Assert
        (await act.Should().ThrowAsync<ValidationAppException>())
            .Which.Code.Should().Be(DomainErrorEnum.MISSING_RESOURCE_PERMISSION);
    }

    [Fact]
    public async Task Handle_Seguranca_SemUsuarioNoContexto_DeveSerBarrado()
    {
        var currentUser = new Mock<IHttpCurrentUser>();
        currentUser.Setup(x => x.GetContextUser()).Returns((UserLoggedViewModel?)null);

        var act = async () => await HandleAsync(currentUser, Query(userType: "Recruiter"));

        (await act.Should().ThrowAsync<ValidationAppException>())
            .Which.Code.Should().Be(DomainErrorEnum.MISSING_RESOURCE_PERMISSION);
    }

    // ---------- Infraestrutura do teste ----------

    private static GetAllUsersQuery Query(
        string? userType = null, bool? isDeleted = null, string? search = null, int page = 1, int size = 100) =>
        new(Page: page, Size: size, OrderBy: null, IsDeleted: isDeleted, Search: search, UserType: userType);

    /// <summary>
    /// Termo único <b>só de letras</b>: termo com dígitos também procura pelo CPF, e poucos dígitos
    /// soltos casariam CPFs aleatórios de outros testes (resultado instável).
    /// </summary>
    private static string Token(string prefix)
    {
        var letters = Guid.NewGuid().ToString("N")
            .Select(c => char.IsDigit(c) ? (char)('g' + (c - '0')) : c)
            .Take(14);
        return prefix + new string(letters.ToArray());
    }

    private static Mock<IHttpCurrentUser> CurrentUserWithRole(string role)
    {
        var currentUser = new Mock<IHttpCurrentUser>();
        currentUser.SetupGet(x => x.UserId).Returns(1);
        currentUser.Setup(x => x.GetContextUser()).Returns(new UserLoggedViewModel
        {
            AccessToken = "token",
            ExpiresIn = 3600,
            UserToken = new UserToken
            {
                Id = 1,
                Username = "current",
                Email = "current@test.local",
                Roles = [role],
                Claims = []
            }
        });
        return currentUser;
    }

    private Task<ListDataPagination<UserViewModel>> HandleAsAdminAsync(GetAllUsersQuery query) =>
        HandleAsync(CurrentUserWithRole(RecruitmentRoleNames.Admin), query);

    private async Task<ListDataPagination<UserViewModel>> HandleAsync(
        Mock<IHttpCurrentUser> currentUser, GetAllUsersQuery query)
    {
        using var scope = _fixture.Services.CreateScope();
        var users = scope.ServiceProvider.GetRequiredService<UserManager<User>>();
        var sut = new GetAllUsersHandler(users, currentUser.Object);
        return await sut.Handle(query, CancellationToken.None);
    }

    /// <summary>Cria usuário com nome de usuário iniciado por <paramref name="namePrefix"/>.</summary>
    private async Task<long> CreateUserAsync(string namePrefix, UserTypeEnum userType, bool deleted = false)
    {
        var id = await AuthIntegrationTestHelper.RegisterConfirmedUserAsync(
            _fixture.Services, TestDataFactory.UniqueEmail(namePrefix), namePrefix);

        using var scope = _fixture.Services.CreateScope();
        var users = scope.ServiceProvider.GetRequiredService<UserManager<User>>();
        var user = await users.FindByIdAsync(id.ToString());
        user!.UserType = userType;
        if (deleted)
        {
            user.IsDeleted = true;
            user.DeletedAt = DateTimeOffset.UtcNow;
        }

        var update = await users.UpdateAsync(user);
        update.Succeeded.Should().BeTrue();
        return id;
    }
}
