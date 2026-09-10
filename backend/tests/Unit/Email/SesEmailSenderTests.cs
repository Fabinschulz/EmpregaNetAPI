using Amazon.SimpleEmailV2;
using Amazon.SimpleEmailV2.Model;
using EmpregaNet.Application.Auth.Configuration;
using EmpregaNet.Infra.Email;
using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;

namespace EmpregaNet.Tests.Unit.Email;

/// <summary>
/// O transporte SES tem de montar o pedido com o que o serviço realmente exige: remetente verificado
/// com nome amigável, corpo HTML em UTF-8 e falha convertida em erro accionável.
/// </summary>
/// <remarks>
/// Estes testes existem porque o pedido é montado <b>uma vez</b> e serve todos os e-mails da aplicação:
/// um <c>Charset</c> em falta ou um <c>From</c> mal formatado não falha na build nem em nenhum outro
/// teste — aparece como acento quebrado na caixa de entrada ou recusa do SES, já em produção.
/// </remarks>
public sealed class SesEmailSenderTests
{
    private readonly Mock<IAmazonSimpleEmailServiceV2> _ses = new();
    private readonly List<SendEmailRequest> _sent = [];

    public SesEmailSenderTests()
    {
        _ses.Setup(x => x.SendEmailAsync(It.IsAny<SendEmailRequest>(), It.IsAny<CancellationToken>()))
            .Callback<SendEmailRequest, CancellationToken>((request, _) => _sent.Add(request))
            .ReturnsAsync(new SendEmailResponse { MessageId = "0100018f-ses-message-id" });
    }

    private static SesEmailOptions NewOptions(string fromName = "EmpregaNet") => new()
    {
        Enabled = true,
        Region = "sa-east-1",
        FromEmail = "noreply@empreganet.test",
        FromName = fromName
    };

    private SesEmailSender CreateSut(SesEmailOptions? options = null) => new(
        _ses.Object,
        Microsoft.Extensions.Options.Options.Create(options ?? NewOptions()),
        NullLogger<SesEmailSender>.Instance);

    private SendEmailRequest OnlyRequest() => _sent.Should().ContainSingle().Subject;

    [Fact]
    public async Task SendEmail_DeveMontarDestinatarioAssuntoECorpoHtmlEmUtf8()
    {
        await CreateSut().SendEmailAsync("candidato@test.local", "Confirme a sua conta", "<p>Olá</p>");

        var request = OnlyRequest();
        request.Destination.ToAddresses.Should().ContainSingle().Which.Should().Be("candidato@test.local");
        request.Content.Simple.Subject.Data.Should().Be("Confirme a sua conta");
        request.Content.Simple.Subject.Charset.Should().Be("UTF-8");
        request.Content.Simple.Body.Html.Data.Should().Be("<p>Olá</p>");
        request.Content.Simple.Body.Html.Charset.Should().Be("UTF-8");
    }

    [Fact]
    public async Task SendEmail_DeveEnviarComNomeAmigavelCitadoNoRemetente()
    {
        await CreateSut().SendEmailAsync("candidato@test.local", "Assunto", "<p>corpo</p>");

        OnlyRequest().FromEmailAddress.Should().Be("\"EmpregaNet\" <noreply@empreganet.test>");
    }

    [Fact]
    public async Task SendEmail_ComNomeAcentuado_DeveCodificarOFromEmRfc2047()
    {
        await CreateSut(NewOptions(fromName: "Vagas São Paulo"))
            .SendEmailAsync("candidato@test.local", "Assunto", "<p>corpo</p>");

        OnlyRequest().FromEmailAddress.Should()
            .Be("=?UTF-8?B?VmFnYXMgU8OjbyBQYXVsbw==?= <noreply@empreganet.test>");
    }

    [Fact]
    public async Task SendEmail_SemNomeConfigurado_DeveUsarSoOEndereco()
    {
        var options = NewOptions();
        options.FromName = string.Empty;

        await CreateSut(options).SendEmailAsync("candidato@test.local", "Assunto", "<p>corpo</p>");

        OnlyRequest().FromEmailAddress.Should().Be("noreply@empreganet.test");
    }

    [Fact]
    public async Task SendEmail_QuandoIdentidadeNaoExisteNaRegiao_DeveApontarParaSesRegion()
    {
        _ses.Setup(x => x.SendEmailAsync(It.IsAny<SendEmailRequest>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new NotFoundException("identity not found"));

        var act = () => CreateSut().SendEmailAsync("candidato@test.local", "Assunto", "<p>corpo</p>");

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*Ses:Region*")
            .WithInnerException<InvalidOperationException, NotFoundException>();
    }

    [Fact]
    public async Task SendEmail_QuandoSesRecusaAMensagem_DeveMencionarOSandbox()
    {
        _ses.Setup(x => x.SendEmailAsync(It.IsAny<SendEmailRequest>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new MessageRejectedException("Email address is not verified"));

        var act = () => CreateSut().SendEmailAsync("candidato@test.local", "Assunto", "<p>corpo</p>");

        (await act.Should().ThrowAsync<InvalidOperationException>()).WithMessage("*sandbox*");
    }
}
