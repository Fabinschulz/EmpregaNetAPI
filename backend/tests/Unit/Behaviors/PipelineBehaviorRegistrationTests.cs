using EmpregaNet.Domain.Interfaces;
using EmpregaNet.Application.Abstraction;
using EmpregaNet.Infra;
using EmpregaNet.Infra.Behaviors;
using EmpregaNet.Infra.Events;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;

namespace EmpregaNet.Tests.Unit.Behaviors;

/// <summary>
/// Garante que o pipeline CQRS se monta corretamente para requisições transacionais e não
/// transacionais, e que <see cref="PerformanceBehaviour{TRequest,TResponse}"/> é a camada externa.
/// </summary>
/// <remarks>
/// O <c>TransactionBehavior</c> tem restrição <c>where TRequest : ITransactional</c>, enquanto os
/// outros dois não. Todos são registrados como genéricos abertos na mesma coleção, então a montagem
/// do pipeline depende de o contêiner descartar silenciosamente a registação cuja restrição a
/// requisição não satisfaz. Se esse comportamento mudar, toda query passaria a estourar ao resolver
/// os behaviors e falha em tempo de execução, em todo endpoint de leitura, que nenhum outro teste
/// apanharia porque a fixture de integração instancia os handlers diretamente, sem passar pelo
/// Mediator.
/// </remarks>
public sealed class PipelineBehaviorRegistrationTests
{
    private sealed record PingQuery : IRequest<string>;

    private sealed record PingCommand : IRequest<string>, ITransactional;

    private static ServiceProvider BuildProvider()
    {
        var services = new ServiceCollection();

        services.AddLogging(b => b.SetMinimumLevel(LogLevel.Warning));
        services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
        services.AddScoped(_ => new Mock<IUnityOfWork>().Object);
        services.AddScoped(_ => new Mock<IMediator>().Object);
        services.AddScoped<IDomainEventQueue, DomainEventQueue>();

        // O registo REAL da API, e não uma cópia da ordem. Recopiar as linhas aqui testaria que a
        // ServiceCollection preserva ordem de inserção — coisa que a plataforma já garante — e
        // continuaria verde com a produção alterada.
        services.AddPipelineBehaviors();

        return services.BuildServiceProvider();
    }

    [Fact]
    public void Query_NaoTransacional_DeveResolverSemTransactionBehavior()
    {
        using var provider = BuildProvider();

        var behaviors = provider.GetServices<IPipelineBehavior<PingQuery, string>>().ToList();

        behaviors.Should().HaveCount(3, "uma query não satisfaz a restrição ITransactional");
        behaviors[0].Should().BeOfType<PerformanceBehaviour<PingQuery, string>>();
        behaviors[1].Should().BeOfType<ValidationBehavior<PingQuery, string>>();
        behaviors[2].Should().BeOfType<NotificationDispatchBehavior<PingQuery, string>>();
    }

    [Fact]
    public void Command_Transacional_DeveResolverOsQuatroBehaviors()
    {
        using var provider = BuildProvider();

        var behaviors = provider.GetServices<IPipelineBehavior<PingCommand, string>>().ToList();

        behaviors.Should().HaveCount(4);
        behaviors[0].Should().BeOfType<PerformanceBehaviour<PingCommand, string>>();
        behaviors[1].Should().BeOfType<ValidationBehavior<PingCommand, string>>();
        behaviors[2].Should().BeOfType<NotificationDispatchBehavior<PingCommand, string>>();
        behaviors[3].Should().BeOfType<TransactionBehavior<PingCommand, string>>();
    }

    /// <summary>
    /// A garantia central da notificação, afirmada sobre o registo <b>de produção</b>: o despacho de
    /// eventos é mais externo que a transacção.
    /// </summary>
    /// <remarks>
    /// O pipeline é montado de trás para frente, portanto índice menor = camada mais externa = observa
    /// o commit já feito. Se alguém inverter as duas linhas em <c>AddPipelineBehaviors</c>, o e-mail
    /// passa a sair de dentro da transacção e um rollback deixa o candidato informado de uma aprovação
    /// que não existe — este teste é o que falha nesse momento.
    /// </remarks>
    [Fact]
    public void Command_DespachoDeEventos_DeveSerMaisExternoQueATransacao()
    {
        using var provider = BuildProvider();

        var behaviors = provider.GetServices<IPipelineBehavior<PingCommand, string>>().ToList();

        var dispatchIndex = behaviors.FindIndex(b => b is NotificationDispatchBehavior<PingCommand, string>);
        var transactionIndex = behaviors.FindIndex(b => b is TransactionBehavior<PingCommand, string>);

        dispatchIndex.Should().BeGreaterThanOrEqualTo(0);
        transactionIndex.Should().BeGreaterThanOrEqualTo(0);
        dispatchIndex.Should().BeLessThan(
            transactionIndex,
            "índice menor = registado antes = camada mais externa = observa a transacção já commitada");
    }

    /// <summary>
    /// O behavior de performance não pode falhar por não haver usuário autenticado.
    /// </summary>
    /// <remarks>
    /// Ele lia o utilizador por <c>IHttpCurrentUser.GetContextUser()</c>, que termina em
    /// <c>?? throw</c>. Como o limiar só é avaliado depois do handler, uma requisição anónima lenta,
    /// o catálogo público de vagas, ou o próprio login, que faz hash de senha, deixava de
    /// devolver a sua resposta e virava 500. O diagnóstico não pode derrubar o pedido observado.
    /// </remarks>
    [Fact]
    public async Task PerformanceBehaviour_SemUsuarioAutenticado_NaoDeveLancar()
    {
        using var provider = BuildProvider();

        var behavior = provider
            .GetServices<IPipelineBehavior<PingQuery, string>>()
            .OfType<PerformanceBehaviour<PingQuery, string>>()
            .Single();

        // Sem HttpContext: é o cenário em que o acesso ao utilizador lançava.
        var act = async () => await behavior.Handle(
            new PingQuery(),
            async () =>
            {
                await Task.Delay(600);
                return "ok";
            },
            CancellationToken.None);

        (await act.Should().NotThrowAsync()).Subject.Should().Be("ok");
    }
}
