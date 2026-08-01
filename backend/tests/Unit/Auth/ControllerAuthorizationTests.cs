using System.Reflection;
using EmpregaNet.Api.Controllers.Admin;
using EmpregaNet.Api.Controllers.Auth;
using EmpregaNet.Api.Controllers.Candidates;
using EmpregaNet.Api.Controllers.Companies;
using EmpregaNet.Api.Controllers.JobApplications;
using EmpregaNet.Api.Controllers.Jobs;
using EmpregaNet.Api.Controllers.Users;
using EmpregaNet.Application.Utils;
using FluentAssertions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EmpregaNet.Tests.Unit.Auth;

/// <summary>
/// Trava as decisões de autorização da superfície HTTP.
/// </summary>
/// <remarks>
/// A verificação é por reflexão em vez de HTTP porque o que precisa ficar preso é a declaração:
/// subir host e bater no endpoint testaria o ASP.NET Core, não a nossa decisão. Aqui, remover um
/// atributo ou rebaixar uma policy quebra o teste na hora.
///
/// <para>Cada controller precisa estar classificado em um dos três grupos. Um controller novo que
/// não entre em nenhum falha o último teste, é o que impede que a superfície cresça sem que
/// alguém decida conscientemente o nível de acesso.</para>
/// </remarks>
public sealed class ControllerAuthorizationTests
{
    /// <summary>Superfície privilegiada: exige papel declarado na classe.</summary>
    private static readonly (Type Controller, string Policy)[] ControllersComPapel =
    [
        (typeof(AdminController), Constants.AuthPolicies.Administrador),
        (typeof(CompaniesController), Constants.AuthPolicies.Administrador),
        (typeof(CandidatesController), Constants.AuthPolicies.Recrutamento)
    ];

    /// <summary>
    /// Superfície autenticada sem papel fixo na classe: auto-serviço do próprio usuário, ou
    /// mistura de auto-serviço com ações privilegiadas declaradas ação a ação.
    /// </summary>
    private static readonly Type[] ControllersAutenticados =
    [
        typeof(UsersController),
        typeof(JobApplicationsController),
        typeof(JobsController)
    ];

    /// <summary>Superfície deliberadamente anônima.</summary>
    private static readonly Type[] ControllersAnonimos = [typeof(AuthController)];

    public static TheoryData<Type, string> SuperficiePrivilegiada
    {
        get
        {
            var data = new TheoryData<Type, string>();

            foreach (var (controller, policy) in ControllersComPapel)
            {
                data.Add(controller, policy);
            }

            return data;
        }
    }

    [Theory]
    [MemberData(nameof(SuperficiePrivilegiada))]
    public void ControllerPrivilegiado_DeveExigirOPapelEsperado(Type controller, string policy)
    {
        PoliciesDeclaradas(controller).Should().Contain(
            policy,
            $"{controller.Name} expõe operações restritas e não pode ficar acessível a qualquer usuário autenticado");
    }

    [Fact]
    public void TodoController_DeveDeclararAutorizacaoOuAnonimatoExplicito()
    {
        var semDecisao = TodosOsControllers()
            .Where(t => !t.GetCustomAttributes<AuthorizeAttribute>(inherit: true).Any()
                        && !t.GetCustomAttributes<AllowAnonymousAttribute>(inherit: true).Any())
            .Select(t => t.Name)
            .ToArray();

        semDecisao.Should().BeEmpty(
            "um controller sem [Authorize] nem [AllowAnonymous] fica público por omissão");
    }

    /// <summary>
    /// No <see cref="JobsController"/> a leitura é pública e a escrita não.
    /// </summary>
    /// <remarks>
    /// É a forma mais arriscada da API: como o controller já contém ações <c>[AllowAnonymous]</c>,
    /// uma ação de escrita nova que esqueça a policy não chama atenção na revisão, parece
    /// coerente com as vizinhas. Este teste exige que toda ação mutante ali declare o papel.
    /// </remarks>
    [Fact]
    public void JobsController_AcoesMutantesDevemExigirRecrutamento()
    {
        var mutantesSemPapel = AcoesMutantes(typeof(JobsController))
            .Where(m => !m.GetCustomAttributes<AuthorizeAttribute>()
                          .Any(a => a.Policy == Constants.AuthPolicies.Recrutamento))
            .Select(m => m.Name)
            .ToArray();

        mutantesSemPapel.Should().BeEmpty(
            "escrita de vaga é operação de recrutamento, mesmo num controller com leitura pública");
    }

    [Fact]
    public void ControllerAnonimo_NaoDeveExporEscritaPrivilegiadaSemPolicy()
    {
        // AuthController é anônimo por desenho (login, registro, refresh). O que não pode é uma
        // ação privilegiada nascer ali e herdar o anonimato da classe sem ninguém notar.
        var acoes = AcoesMutantes(typeof(AuthController)).Select(m => m.Name).ToArray();

        acoes.Should().OnlyContain(
            nome => EndpointsAnonimosEsperados.Contains(nome),
            "toda ação anônima do AuthController precisa ser uma decisão explícita registrada aqui");
    }

    private static readonly string[] EndpointsAnonimosEsperados =
    [
        nameof(AuthController.Register),
        nameof(AuthController.Login),
        nameof(AuthController.RefreshToken),
        nameof(AuthController.Logout),
        nameof(AuthController.LoginWithGoogle),
        nameof(AuthController.ForgotPassword),
        nameof(AuthController.ConfirmEmail),
        nameof(AuthController.ResendEmailConfirmation),
        nameof(AuthController.ResetPassword)
    ];

    [Fact]
    public void TodoController_DeveEstarClassificado()
    {
        var conhecidos = ControllersComPapel.Select(c => c.Controller)
            .Concat(ControllersAutenticados)
            .Concat(ControllersAnonimos)
            .ToHashSet();

        var naoClassificados = TodosOsControllers()
            .Where(t => !conhecidos.Contains(t))
            .Select(t => t.Name)
            .ToArray();

        naoClassificados.Should().BeEmpty(
            "controller novo precisa ser classificado como privilegiado, autenticado ou anônimo");
    }

    private static string[] PoliciesDeclaradas(Type controller) =>
        controller.GetCustomAttributes<AuthorizeAttribute>(inherit: true)
            .Select(a => a.Policy)
            .Where(p => !string.IsNullOrWhiteSpace(p))
            .Select(p => p!)
            .ToArray();

    private static IEnumerable<MethodInfo> AcoesMutantes(Type controller) =>
        controller.GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly)
            .Where(m => m.GetCustomAttributes<HttpPostAttribute>().Any()
                        || m.GetCustomAttributes<HttpPutAttribute>().Any()
                        || m.GetCustomAttributes<HttpPatchAttribute>().Any()
                        || m.GetCustomAttributes<HttpDeleteAttribute>().Any());

    private static IEnumerable<Type> TodosOsControllers() =>
        typeof(AuthController).Assembly
            .GetTypes()
            .Where(t => typeof(ControllerBase).IsAssignableFrom(t) && !t.IsAbstract);
}
