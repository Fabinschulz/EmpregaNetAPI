using EmpregaNet.Application.Abstraction;
using EmpregaNet.Domain.Interfaces;
using EmpregaNet.Infra.Behaviors;
using EmpregaNet.Infra.Events;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;

namespace EmpregaNet.Tests.Unit.Behaviors;

/// <summary>
/// A garantia central da notificação: <b>nada é publicado sobre uma operação que não foi
/// confirmada</b>. Um e-mail não se desfaz — se sair sobre uma transacção revertida, o candidato fica
/// a saber de uma aprovação que não existe, e não há correcção possível do lado do produto.
/// </summary>
public sealed class NotificationDispatchBehaviorTests
{
    private sealed record TestEvent(string Name) : INotification;

    private sealed record PingCommand : IRequest<string>, ITransactional;

    private readonly Mock<IMediator> _mediator = new();
    private readonly DomainEventQueue _queue = new();

    private NotificationDispatchBehavior<PingCommand, string> CreateSut() =>
        new(_queue, _mediator.Object, NullLogger<NotificationDispatchBehavior<PingCommand, string>>.Instance);

    [Fact]
    public async Task Handle_RequisicaoConcluida_DevePublicarOQueOHandlerEnfileirou()
    {
        _queue.Enqueue(new TestEvent("aprovada"));

        var result = await CreateSut().Handle(new PingCommand(), () => Task.FromResult("ok"), CancellationToken.None);

        result.Should().Be("ok");
        _mediator.Verify(
            x => x.Publish(It.Is<TestEvent>(e => e.Name == "aprovada"), It.IsAny<CancellationToken>()),
            Times.Once);
    }

    /// <summary>
    /// O caso que justifica o desenho todo: o handler enfileirou, a transacção rebentou depois, e
    /// <b>nenhum</b> e-mail sai.
    /// </summary>
    [Fact]
    public async Task Handle_RequisicaoFalhou_NaoDevePublicarNada()
    {
        _queue.Enqueue(new TestEvent("aprovada"));

        var act = async () => await CreateSut().Handle(
            new PingCommand(),
            () => throw new InvalidOperationException("rollback da transacção"),
            CancellationToken.None);

        await act.Should().ThrowAsync<InvalidOperationException>();
        _mediator.Verify(
            x => x.Publish(It.IsAny<TestEvent>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    // Falha de e-mail depois do commit não pode virar erro de resposta: a transição está gravada, e
    // devolver erro faria o recrutador repeti-la (W1/W2).
    [Fact]
    public async Task Handle_PublicacaoFalha_DeveDevolverOResultadoSemRelancar()
    {
        _queue.Enqueue(new TestEvent("aprovada"));
        _mediator
            .Setup(x => x.Publish(It.IsAny<TestEvent>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("servidor de e-mail em baixo"));

        var result = await CreateSut().Handle(new PingCommand(), () => Task.FromResult("ok"), CancellationToken.None);

        result.Should().Be("ok");
    }

    [Fact]
    public async Task Handle_FilaVazia_NaoDevePublicarNada()
    {
        await CreateSut().Handle(new PingCommand(), () => Task.FromResult("ok"), CancellationToken.None);

        _mediator.Verify(
            x => x.Publish(It.IsAny<TestEvent>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task Handle_DuasPassagensNoMesmoEscopo_NaoDeveRepublicarOQueJaSaiu()
    {
        _queue.Enqueue(new TestEvent("aprovada"));
        var sut = CreateSut();

        await sut.Handle(new PingCommand(), () => Task.FromResult("ok"), CancellationToken.None);
        await sut.Handle(new PingCommand(), () => Task.FromResult("ok"), CancellationToken.None);

        _mediator.Verify(
            x => x.Publish(It.IsAny<TestEvent>(), It.IsAny<CancellationToken>()),
            Times.Once);
    }

    private sealed record PublishProbe(string Name) : INotification;

    private sealed class ProbeHandler : INotificationHandler<PublishProbe>
    {
        public static readonly List<string> Received = [];

        public Task Handle(PublishProbe notification, CancellationToken cancellationToken)
        {
            Received.Add(notification.Name);
            return Task.CompletedTask;
        }
    }

    private sealed record ProbeCommand : IRequest<string>, ITransactional;

    private sealed class ProbeCommandHandler : IRequestHandler<ProbeCommand, string>
    {
        private readonly IDomainEventQueue _queue;

        public ProbeCommandHandler(IDomainEventQueue queue) => _queue = queue;

        public Task<string> Handle(ProbeCommand request, CancellationToken cancellationToken)
        {
            _queue.Enqueue(new PublishProbe("do handler"));
            return Task.FromResult("ok");
        }
    }

    /// <summary>
    /// Atravessa o <c>Mediator</c> real: comando enfileira dentro do pipeline, e o handler concreto da
    /// notificação recebe.
    /// </summary>
    /// <remarks>
    /// Os testes acima usam um <c>IMediator</c> falsificado, e por isso não provariam o ponto mais
    /// fácil de errar: <c>Publish</c> é genérico no tipo da notificação, e publicar como
    /// <c>INotification</c> não encontraria handler nenhum — as notificações desapareceriam em
    /// silêncio, com todos os outros testes verdes.
    /// </remarks>
    [Fact]
    public async Task Pipeline_ComMediatorReal_DeveEntregarAoHandlerConcretoDaNotificacao()
    {
        ProbeHandler.Received.Clear();

        var services = new ServiceCollection();
        services.AddLogging(b => b.SetMinimumLevel(LogLevel.Warning));
        services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
        services.AddScoped<IDomainEventQueue, DomainEventQueue>();
        // AddMediator registra o Mediator real (internal no Domain) e descobre os handlers do
        // assembly de testes, incluindo os deste ficheiro.
        services.AddMediator(typeof(NotificationDispatchBehaviorTests).Assembly);
        services.AddScoped(typeof(IPipelineBehavior<,>), typeof(NotificationDispatchBehavior<,>));

        using var provider = services.BuildServiceProvider();
        using var scope = provider.CreateScope();

        var result = await scope.ServiceProvider.GetRequiredService<IMediator>()
            .Send(new ProbeCommand(), CancellationToken.None);

        result.Should().Be("ok");
        ProbeHandler.Received.Should().Equal("do handler");
    }
}
