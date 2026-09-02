using EmpregaNet.Domain.Entities;
using EmpregaNet.Domain.Enums;
using FluentAssertions;

namespace EmpregaNet.Tests.Unit.Domain.Jobs;

/// <summary>
/// Invariantes do agregado <see cref="JobApplication"/>: em que estado a candidatura nasce, quando o
/// candidato ainda pode desistir, e o que o recrutamento não pode sobrepor.
/// </summary>
public sealed class JobApplicationAggregateTests
{
    private const long JobId = 42;
    private const long UserId = 7;

    private static JobApplication CreateApplication(ApplicationStatusEnum? status = null)
    {
        var application = new JobApplication(JobId, UserId);

        if (status is null || status == ApplicationStatusEnum.Pending)
            return application;

        if (status == ApplicationStatusEnum.CanceledByCandidate)
        {
            application.CancelByCandidate();
            return application;
        }

        application.ChangeStatus(status.Value);
        return application;
    }

    // A candidatura nascia em "Em Análise", o que afirmava ao candidato uma análise que ninguém
    // tinha começado — e deixava o filtro "Recebida" sem nunca devolver resultado.
    [Fact]
    public void Construtor_DeveNascerRecebidaEComDataDeCandidatura()
    {
        var application = CreateApplication();

        application.Status.Should().Be(ApplicationStatusEnum.Pending);
        application.AppliedAt.Should().BeCloseTo(DateTimeOffset.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Theory]
    [InlineData(ApplicationStatusEnum.Pending)]
    [InlineData(ApplicationStatusEnum.Processing)]
    public void CancelByCandidate_ProcessoEmAberto_DeveCancelar(ApplicationStatusEnum status)
    {
        var application = CreateApplication(status);

        application.CancelByCandidate();

        application.Status.Should().Be(ApplicationStatusEnum.CanceledByCandidate);
    }

    // Cancelar depois de um desfecho comunicado apagaria o desfecho: o candidato "desistiria" de uma
    // aprovação que já recebeu, e o recrutador veria a candidatura sair do funil sem explicação.
    [Theory]
    [InlineData(ApplicationStatusEnum.Approved)]
    [InlineData(ApplicationStatusEnum.Rejected)]
    [InlineData(ApplicationStatusEnum.Finished)]
    [InlineData(ApplicationStatusEnum.Canceled)]
    [InlineData(ApplicationStatusEnum.Timeout)]
    [InlineData(ApplicationStatusEnum.Error)]
    [InlineData(ApplicationStatusEnum.CanceledByCandidate)]
    public void CancelByCandidate_EstadoComDesfecho_DeveRecusar(ApplicationStatusEnum status)
    {
        var application = CreateApplication(status);

        var act = () => application.CancelByCandidate();

        act.Should().Throw<InvalidOperationException>()
            .WithMessage("Esta candidatura não pode mais ser cancelada.");
        application.Status.Should().Be(status);
    }

    // Segunda tentativa do mesmo cancelamento falha antes de qualquer efeito: é o que dispensa
    // tabela de controlo para não notificar duas vezes o mesmo ato.
    [Fact]
    public void CancelByCandidate_RepetidoNaMesmaCandidatura_DeveRecusarASegundaVez()
    {
        var application = CreateApplication(ApplicationStatusEnum.Processing);
        application.CancelByCandidate();

        var act = () => application.CancelByCandidate();

        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void ChangeStatus_DestinoCanceladoPeloCandidato_DeveRecusar()
    {
        var application = CreateApplication(ApplicationStatusEnum.Processing);

        var act = () => application.ChangeStatus(ApplicationStatusEnum.CanceledByCandidate);

        act.Should().Throw<InvalidOperationException>()
            .WithMessage("O cancelamento pelo candidato não pode ser aplicado pelo recrutamento.");
        application.Status.Should().Be(ApplicationStatusEnum.Processing);
    }

    [Theory]
    [InlineData(ApplicationStatusEnum.Processing)]
    [InlineData(ApplicationStatusEnum.Approved)]
    [InlineData(ApplicationStatusEnum.Rejected)]
    [InlineData(ApplicationStatusEnum.Canceled)]
    public void ChangeStatus_APartirDeCanceladoPeloCandidato_DeveRecusar(ApplicationStatusEnum destino)
    {
        var application = CreateApplication(ApplicationStatusEnum.CanceledByCandidate);

        var act = () => application.ChangeStatus(destino);

        act.Should().Throw<InvalidOperationException>()
            .WithMessage("Esta candidatura foi cancelada pelo candidato e não pode mudar de status.");
        application.Status.Should().Be(ApplicationStatusEnum.CanceledByCandidate);
    }

    // Repetir a mesma transição não é um segundo evento: é uma operação inválida. É o que impede o
    // recrutador de gerar duas notificações idênticas clicando duas vezes.
    [Fact]
    public void ChangeStatus_StatusIgualAoActual_DeveRecusar()
    {
        var application = CreateApplication(ApplicationStatusEnum.Processing);

        var act = () => application.ChangeStatus(ApplicationStatusEnum.Processing);

        act.Should().Throw<InvalidOperationException>()
            .WithMessage("A candidatura já está no status informado.");
    }

    /// <summary>
    /// A coluna <c>Status</c> é <c>integer</c> e o enum não declara valores explícitos: a posição de
    /// cada membro <b>é</b> o dado gravado. Este teste falha se alguém inserir um valor no meio, que
    /// é a única forma de reescrever em silêncio o status de candidaturas já existentes.
    /// </summary>
    [Fact]
    public void ApplicationStatusEnum_OrdemDosValores_DeveSerEstavel()
    {
        ((int)ApplicationStatusEnum.NaoSelecionado).Should().Be(0);
        ((int)ApplicationStatusEnum.Approved).Should().Be(1);
        ((int)ApplicationStatusEnum.Pending).Should().Be(2);
        ((int)ApplicationStatusEnum.Rejected).Should().Be(3);
        ((int)ApplicationStatusEnum.Timeout).Should().Be(4);
        ((int)ApplicationStatusEnum.Canceled).Should().Be(5);
        ((int)ApplicationStatusEnum.Error).Should().Be(6);
        ((int)ApplicationStatusEnum.Processing).Should().Be(7);
        ((int)ApplicationStatusEnum.Finished).Should().Be(8);
        ((int)ApplicationStatusEnum.CanceledByCandidate).Should().Be(9);
    }
}
