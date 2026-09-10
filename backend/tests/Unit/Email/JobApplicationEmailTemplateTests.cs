using EmpregaNet.Application.JobApplications.Events;
using EmpregaNet.Domain.Enums;
using EmpregaNet.Infra.Email;
using FluentAssertions;

namespace EmpregaNet.Tests.Unit.Email;

/// <summary>
/// Conteúdo mínimo do e-mail de andamento (W4/CA-01): vaga, empresa, situação e data. Um e-mail que
/// diz "houve novidade" sem dizer em qual candidatura obriga o candidato a entrar na plataforma para
/// descobrir de que se trata — e ele pode ter várias candidaturas em curso.
/// </summary>
public sealed class JobApplicationEmailTemplateTests
{
    private const string JobTitle = "Operador de Empilhadeira";
    private const string CompanyName = "Metalurgica Extrema";

    private static (string Subject, string HtmlBody) Render(
        JobApplicationNotificationReason reason = JobApplicationNotificationReason.StatusChanged,
        ApplicationStatusEnum newStatus = ApplicationStatusEnum.Approved,
        string candidateName = "Ana",
        string statusDescription = "Aprovado",
        string jobTitle = JobTitle) =>
        EmpregaNetEmailTemplates.JobApplicationStatus(
            candidateName: candidateName,
            jobTitle: jobTitle,
            companyName: CompanyName,
            statusDescription: statusDescription,
            newStatus: newStatus,
            occurredAt: "02/09/2026 09:30:00",
            applicationsLink: "https://app.test/candidaturas",
            reason: reason);

    [Fact]
    public void JobApplicationStatus_DeveConterVagaEmpresaSituacaoEData()
    {
        var (_, html) = Render();

        html.Should().Contain(JobTitle);
        html.Should().Contain(CompanyName);
        html.Should().Contain("Aprovado");
        html.Should().Contain("02/09/2026 09:30:00");
    }

    [Fact]
    public void JobApplicationStatus_DeveApontarParaAAreaDeCandidaturas()
    {
        var (_, html) = Render();

        html.Should().Contain("https://app.test/candidaturas");
        html.Should().Contain("Ver minhas candidaturas");
    }

    /// <summary>
    /// A <b>razão</b> escolhe o assunto. Usa um status sem tratamento próprio (<c>Timeout</c>) para
    /// isolar essa escolha da ramificação por status, que é testada à parte.
    /// </summary>
    [Theory]
    [InlineData(JobApplicationNotificationReason.Applied, "Candidatura enviada")]
    [InlineData(JobApplicationNotificationReason.StatusChanged, "Atualização da candidatura")]
    [InlineData(JobApplicationNotificationReason.JobClosed, "Vaga encerrada")]
    [InlineData(JobApplicationNotificationReason.CanceledByCandidate, "Candidatura cancelada")]
    public void JobApplicationStatus_AssuntoDeveIdentificarOMotivoEAVaga(
        JobApplicationNotificationReason reason,
        string expectedPrefix)
    {
        var (subject, _) = Render(reason, newStatus: ApplicationStatusEnum.Timeout, statusDescription: "Expirado");

        subject.Should().Contain(expectedPrefix);
        subject.Should().Contain(JobTitle);
    }

    /// <summary>
    /// O título da vaga é texto que um recrutador escreveu, e entra num corpo HTML. Sem escape, um
    /// título com marcação injectaria conteúdo no e-mail de todos os candidatos daquela vaga.
    /// </summary>
    [Fact]
    public void JobApplicationStatus_DeveEscaparHtmlVindoDoTituloDaVaga()
    {
        var (_, html) = Render(jobTitle: "<script>alert(1)</script>");

        html.Should().NotContain("<script>alert(1)</script>");
        html.Should().Contain("&lt;script&gt;");
    }

    /// <summary>
    /// Texto acentuado sai como entidade numérica (<c>Metal&amp;#250;rgica</c>), não literal. É o que
    /// <c>WebUtility.HtmlEncode</c> faz e é correcto para e-mail — os clientes renderizam a entidade.
    /// Fica fixado para ninguém "corrigir" o escape ao ver entidades no fonte do e-mail.
    /// </summary>
    [Fact]
    public void JobApplicationStatus_TextoAcentuado_DeveSairComoEntidadeHtml()
    {
        var (_, html) = EmpregaNetEmailTemplates.JobApplicationStatus(
            candidateName: "Ana",
            jobTitle: "Operador de Empilhadeira",
            companyName: "Metalúrgica Extrema",
            statusDescription: "Em Análise",
            newStatus: ApplicationStatusEnum.Processing,
            occurredAt: "02/09/2026 09:30:00",
            applicationsLink: "https://app.test/candidaturas",
            reason: JobApplicationNotificationReason.StatusChanged);

        html.Should().Contain("Metal&#250;rgica Extrema");
        html.Should().NotContain("Metalúrgica Extrema");
    }

    /// <summary>
    /// Aprovada e reprovada chegam ambas como <c>StatusChanged</c>. Se o template só ramificasse pela
    /// razão, a recusa sairia com o verde e o sino da aprovação — lida como boa notícia antes de o
    /// texto ser lido.
    /// </summary>
    [Fact]
    public void JobApplicationStatus_AprovadaEReprovada_NaoDevemPartilharOMesmoTom()
    {
        var (aprovadaSubject, aprovadaHtml) = Render(newStatus: ApplicationStatusEnum.Approved, statusDescription: "Aprovado");
        var (reprovadaSubject, reprovadaHtml) = Render(newStatus: ApplicationStatusEnum.Rejected, statusDescription: "Rejeitado");

        aprovadaSubject.Should().NotBe(reprovadaSubject);

        // Verde de aprovação (#16a34a) não pode aparecer numa recusa.
        aprovadaHtml.Should().Contain("#16a34a");
        reprovadaHtml.Should().NotContain("#16a34a");
    }

    [Theory]
    [InlineData(ApplicationStatusEnum.Approved, "aprovada")]
    [InlineData(ApplicationStatusEnum.Rejected, "seguir com ela desta vez")]
    [InlineData(ApplicationStatusEnum.Processing, "analisar a sua candidatura")]
    [InlineData(ApplicationStatusEnum.Finished, "processo seletivo")]
    public void JobApplicationStatus_DeveDizerOQueAconteceuEmCadaStatus(
        ApplicationStatusEnum status,
        string expectedPhrase)
    {
        var (_, html) = Render(newStatus: status);

        html.Should().Contain(expectedPhrase);
    }

    // Status sem tratamento próprio (Timeout, Error) não devem escolher uma emoção: tom neutro e o
    // estado escrito por extenso.
    [Fact]
    public void JobApplicationStatus_StatusSemTratamentoProprio_DeveUsarTomNeutro()
    {
        var (_, html) = Render(newStatus: ApplicationStatusEnum.Timeout, statusDescription: "Expirado");

        html.Should().Contain("Expirado");
        html.Should().NotContain("#16a34a");
    }

    [Fact]
    public void JobApplicationStatus_ComNomeDoCandidato_DeveSaudarPeloNome()
    {
        var (_, html) = Render(candidateName: "Ana");

        html.Should().Contain("Olá, Ana!");
    }

    // Utilizador sem nome existe; "Olá, !" é pior do que não saudar.
    [Fact]
    public void JobApplicationStatus_SemNomeDoCandidato_NaoDeveSaudar()
    {
        var (_, html) = Render(candidateName: "  ");

        html.Should().NotContain("Olá,");
    }
}
