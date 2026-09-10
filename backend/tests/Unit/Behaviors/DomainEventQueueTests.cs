using EmpregaNet.Application.Abstraction;
using EmpregaNet.Application.JobApplications.Events;
using EmpregaNet.Domain.Enums;
using EmpregaNet.Infra.Events;
using FluentAssertions;

namespace EmpregaNet.Tests.Unit.Behaviors;

/// <summary>
/// A fila é um <b>conjunto</b> de factos, não um registo de chamadas.
/// </summary>
/// <remarks>
/// Defesa em profundidade sobre as invariantes do domínio, que já recusam repetir uma transição
/// (<c>ChangeStatus</c> rejeita o status actual). O que estes testes fixam é a propriedade da fila:
/// declarar duas vezes o mesmo facto na mesma requisição produz <b>um</b> efeito externo. Um e-mail a
/// mais não se desfaz, e a barreira custa um <c>HashSet</c>.
/// </remarks>
public sealed class DomainEventQueueTests
{
    private sealed record PlainEvent(string Name) : INotification;

    private readonly DomainEventQueue _queue = new();

    private static JobApplicationStatusChanged CreateEvent(
        long applicationId = 1,
        ApplicationStatusEnum newStatus = ApplicationStatusEnum.Approved,
        JobApplicationNotificationReason reason = JobApplicationNotificationReason.StatusChanged,
        DateTimeOffset? occurredAt = null) => new(
        JobApplicationId: applicationId,
        JobId: 10,
        CandidateUserId: 20,
        PreviousStatus: ApplicationStatusEnum.Processing,
        NewStatus: newStatus,
        OccurredAt: occurredAt ?? DateTimeOffset.UtcNow,
        Reason: reason);

    /// <summary>
    /// O mesmo facto declarado duas vezes, com instantes diferentes, é um facto só. É o cenário que
    /// justifica <c>OccurredAt</c> ficar fora da chave: se este teste falhar, dois objectos que
    /// descrevem a mesma transição viram dois e-mails.
    /// </summary>
    [Fact]
    public void Enqueue_MesmoFactoComInstantesDiferentes_DeveGuardarUmaVezSo()
    {
        var primeiraTentativa = CreateEvent(occurredAt: new DateTimeOffset(2026, 9, 2, 10, 0, 0, TimeSpan.Zero));
        var segundaTentativa = CreateEvent(occurredAt: new DateTimeOffset(2026, 9, 2, 10, 0, 5, TimeSpan.Zero));

        _queue.Enqueue(primeiraTentativa);
        _queue.Enqueue(segundaTentativa);

        var drained = _queue.Drain();

        drained.Should().ContainSingle();
        drained[0].Should().Be(primeiraTentativa, "fica o primeiro declarado; o segundo é a mesma coisa");
    }

    // Candidaturas diferentes na mesma requisição são factos diferentes: é o caso do encerramento de
    // vaga, que notifica uma por candidatura afectada.
    [Fact]
    public void Enqueue_CandidaturasDiferentes_DeveGuardarTodas()
    {
        _queue.Enqueue(CreateEvent(applicationId: 1));
        _queue.Enqueue(CreateEvent(applicationId: 2));
        _queue.Enqueue(CreateEvent(applicationId: 3));

        _queue.Drain().Should().HaveCount(3);
    }

    [Fact]
    public void Enqueue_MesmaCandidaturaComStatusDiferente_DeveGuardarOsDois()
    {
        _queue.Enqueue(CreateEvent(newStatus: ApplicationStatusEnum.Processing));
        _queue.Enqueue(CreateEvent(newStatus: ApplicationStatusEnum.Approved));

        _queue.Drain().Should().HaveCount(2);
    }

    [Fact]
    public void Enqueue_MesmaCandidaturaComRazaoDiferente_DeveGuardarOsDois()
    {
        _queue.Enqueue(CreateEvent(reason: JobApplicationNotificationReason.StatusChanged));
        _queue.Enqueue(CreateEvent(reason: JobApplicationNotificationReason.JobClosed));

        _queue.Drain().Should().HaveCount(2);
    }

    [Fact]
    public void Drain_DeveEsvaziarTambemAsIdentidades()
    {
        var evento = CreateEvent();
        _queue.Enqueue(evento);
        _queue.Drain();

        // Requisição nova, ou segunda operação no mesmo escopo: o facto pode voltar a ser declarado.
        _queue.Enqueue(evento);

        _queue.Drain().Should().ContainSingle();
    }

    [Fact]
    public void Drain_DevePreservarAOrdemDeEntrada()
    {
        _queue.Enqueue(CreateEvent(applicationId: 7));
        _queue.Enqueue(CreateEvent(applicationId: 8));

        var drained = _queue.Drain().Cast<JobApplicationStatusChanged>().ToList();

        drained.Select(e => e.JobApplicationId).Should().Equal(7L, 8L);
    }

    // Evento sem chave declarada cai na igualdade estrutural do record, que para um record sem campos
    // voláteis é exactamente a identidade certa.
    [Fact]
    public void Enqueue_EventoSemChaveDeclarada_DeveUsarIgualdadeEstrutural()
    {
        _queue.Enqueue(new PlainEvent("a"));
        _queue.Enqueue(new PlainEvent("a"));
        _queue.Enqueue(new PlainEvent("b"));

        _queue.Drain().Should().HaveCount(2);
    }

    [Fact]
    public void Drain_FilaVazia_DeveDevolverListaVazia()
    {
        _queue.Drain().Should().BeEmpty();
    }
}
