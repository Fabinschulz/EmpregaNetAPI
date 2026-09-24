using EmpregaNet.Application.Abstraction;
using EmpregaNet.Application.Admin.Users.Commands;
using EmpregaNet.Application.Auth;
using EmpregaNet.Application.Auth.ViewModel;
using EmpregaNet.Application.Common.Base;
using EmpregaNet.Application.Common.Exceptions;
using EmpregaNet.Application.Users.ViewModel;
using EmpregaNet.Application.Utils.Helpers;
using EmpregaNet.Domain.Entities;
using EmpregaNet.Domain.Enums;
using EmpregaNet.Tests.Support;
using FluentAssertions;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;

namespace EmpregaNet.Tests.Integration.Handlers;

/// <summary>
/// <c>PUT /api/admin/{id}</c>: caminho de escrita do tipo de usuário. É o mais grave dos três pontos
/// que usavam <c>Enum.TryParse</c> frouxo, porque um valor inexistente ("99") era persistido em
/// <c>Users.UserType</c> e repassado ao <see cref="EmpregaNet.Application.Users.Identity.UserTypeRoleSync"/>.
/// </summary>
/// <remarks>
/// Integração porque o handler depende de <c>UserManager&lt;User&gt;</c>/<c>RoleManager&lt;Role&gt;</c>
/// reais do Identity sobre o contexto InMemory, como em <c>GetAllUsersHandlerIntegrationTests</c>.
/// </remarks>
[Collection("Integration")]
public sealed class UpdateAdminUserHandlerIntegrationTests
{
    private readonly InMemoryIdentityFixture _fixture;

    public UpdateAdminUserHandlerIntegrationTests(InMemoryIdentityFixture fixture)
    {
        _fixture = fixture;
    }

    [Theory]
    [InlineData("Recruiter", UserTypeEnum.Recruiter)]
    [InlineData("recruiter", UserTypeEnum.Recruiter)]
    [InlineData("RECRUITER", UserTypeEnum.Recruiter)]
    [InlineData(" Admin ", UserTypeEnum.Admin)] // espaços nas pontas
    public async Task Handle_UserTypeValidoEmQualquerCaixa_DeveAtualizarOTipo(string userType, UserTypeEnum expected)
    {
        var userId = await CreateUserAsync(Token("valido"), UserTypeEnum.Candidate);

        var result = await HandleAsync(userId, userType);

        result.UserType.Should().Be(expected.ToDescription());

        using var scope = _fixture.Services.CreateScope();
        var users = scope.ServiceProvider.GetRequiredService<UserManager<User>>();
        var persisted = await users.FindByIdAsync(userId.ToString());
        persisted!.UserType.Should().Be(expected);
    }

    // Regressão: "99" e "1" passam no Enum.TryParse (aceita número); "Candidate,Recruiter" faria OU
    // bit a bit (= Admin); "Administrador" é o rótulo pt-BR, não o nome do enum. Sem a recusa
    // explícita, qualquer um destes seria persistido em Users.UserType.
    [Theory]
    [InlineData("99")]
    [InlineData("0")]
    [InlineData("1")]
    [InlineData("Candidate,Recruiter")]
    [InlineData("NaoSelecionado")]
    [InlineData("naoselecionado")]
    [InlineData("Administrador")]
    [InlineData("Inexistente")]
    public async Task Handle_UserTypeInvalido_DeveLancarSemAlterarOUsuario(string userType)
    {
        var userId = await CreateUserAsync(Token("invalido"), UserTypeEnum.Candidate);

        var act = async () => await HandleAsync(userId, userType);

        (await act.Should().ThrowAsync<ValidationAppException>())
            .Which.Code.Should().Be(DomainErrorEnum.INVALID_PARAMS);

        using var scope = _fixture.Services.CreateScope();
        var users = scope.ServiceProvider.GetRequiredService<UserManager<User>>();
        var persisted = await users.FindByIdAsync(userId.ToString());
        persisted!.UserType.Should().Be(UserTypeEnum.Candidate);
    }

    // ---------- Infraestrutura do teste ----------

    private static string Token(string prefix) => prefix + Guid.NewGuid().ToString("N")[..10];

    private static Mock<IHttpCurrentUser> AdminCurrentUser()
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
                Username = "admin",
                Email = "admin@test.local",
                Roles = [RecruitmentRoleNames.Admin],
                Claims = []
            }
        });
        return currentUser;
    }

    private async Task<UserViewModel> HandleAsync(long userId, string userType)
    {
        using var scope = _fixture.Services.CreateScope();
        var users = scope.ServiceProvider.GetRequiredService<UserManager<User>>();
        var roles = scope.ServiceProvider.GetRequiredService<RoleManager<Role>>();
        var refreshTokens = new Mock<IRefreshTokenService>();
        var cache = new Mock<IOutputCacheManager>();

        var sut = new UpdateAdminUserHandler(
            users,
            roles,
            AdminCurrentUser().Object,
            refreshTokens.Object,
            cache.Object,
            NullLogger<UpdateAdminUserHandler>.Instance);

        var command = new UpdateCommand<UpdateAdminUserCommand, UserViewModel>(userId, new UpdateAdminUserCommand(userType));
        return await sut.Handle(command, CancellationToken.None);
    }

    private async Task<long> CreateUserAsync(string namePrefix, UserTypeEnum userType)
    {
        var id = await AuthIntegrationTestHelper.RegisterConfirmedUserAsync(
            _fixture.Services, TestDataFactory.UniqueEmail(namePrefix), namePrefix);

        using var scope = _fixture.Services.CreateScope();
        var users = scope.ServiceProvider.GetRequiredService<UserManager<User>>();
        var user = await users.FindByIdAsync(id.ToString());
        user!.UserType = userType;

        var update = await users.UpdateAsync(user);
        update.Succeeded.Should().BeTrue();
        return id;
    }
}
