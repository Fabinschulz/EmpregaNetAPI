using EmpregaNet.Api.Configuration;
using EmpregaNet.Application.Jobs.ViewModel;
using EmpregaNet.Domain.Enums;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace EmpregaNet.Tests.Unit.Api;

/// <summary>
/// Objetivo: travar o contrato JSON das respostas dos controllers - enum pelo nome, propriedade em
/// camelCase.
/// </summary>
/// <remarks>
/// Este teste existe por causa de um defeito de configuração silencioso: o
/// <c>AddJsonOptions(... JsonStringEnumConverter)</c> em <c>RegisterApiDependencies</c> configura o
/// System.Text.Json, que o MVC deixa de usar assim que <c>AddNewtonsoftJson</c> assume os
/// formatters. A configuração lia-se como correta e não tinha efeito nenhum: os enums saíam por
/// índice, e o cliente teve de passar a espelhar a ordem de cada enum à mão para os traduzir.
///
/// <para>O índice é o contrato frágil que se quer evitar: inserir um membro no meio de um enum
/// reescreve o significado de todos os seguintes sem quebrar nada visível no servidor. Se alguém
/// remover o conversor, ou trocar o serializador do MVC outra vez, é aqui que se apanha.</para>
/// </remarks>
public sealed class ResponseJsonContractTests
{
    /// <summary>Serializa como o MVC serializa: com a configuração de produção.</summary>
    private static string SerializeAsController(object payload)
    {
        var options = new MvcNewtonsoftJsonOptions();
        ResponseJsonConfig.Configure(options);

        return JsonConvert.SerializeObject(payload, options.SerializerSettings);
    }

    private static JobViewModel UmaVaga() => new()
    {
        Id = 7,
        Title = "Operador de Empilhadeira",
        Description = "Descrição da vaga.",
        JobType = JobTypeEnum.FullTime,
        WorkModel = WorkModelEnum.OnSite,
        WorkShift = WorkShiftEnum.PrimeiroTurno,
        ExperienceLevel = ExperienceLevelEnum.AteUmAno,
        Area = JobAreaEnum.Logistica,
        City = "Betim",
        State = UF.MG,
        Country = "Brasil",
        Requirements = [],
        Benefits = [],
        PublicationDate = "17/08/2026 09:00:00",
        CompanyId = 3,
        IsActive = true
    };

    [Fact]
    public void EnumDeResposta_DeveSairPeloNome_NaoPeloIndice()
    {
        var json = SerializeAsController(UmaVaga());

        json.Should().Contain("\"jobType\":\"FullTime\"");
        json.Should().Contain("\"workModel\":\"OnSite\"");
        json.Should().Contain("\"workShift\":\"PrimeiroTurno\"");
        json.Should().Contain("\"experienceLevel\":\"AteUmAno\"");
        json.Should().Contain("\"area\":\"Logistica\"");
        json.Should().Contain("\"state\":\"MG\"");
    }

    /// <summary>
    /// O índice do enum não pode voltar a aparecer. `JobTypeEnum.FullTime` é 1: sem o conversor,
    /// o JSON traz `"jobType":1`, que é indistinguível de qualquer outro membro em posição 1.
    /// </summary>
    [Fact]
    public void EnumDeResposta_NaoDeveSairComoNumero()
    {
        var json = SerializeAsController(UmaVaga());

        json.Should().NotContain($"\"jobType\":{(int)JobTypeEnum.FullTime}");
        json.Should().NotContain($"\"state\":{(int)UF.MG}");
    }

    /// <summary>
    /// O camelCase vem por omissão do `AddNewtonsoftJson`; o cliente depende dele em todos os
    /// schemas de leitura.
    /// </summary>
    [Fact]
    public void PropriedadeDeResposta_DeveSairEmCamelCase()
    {
        var json = SerializeAsController(UmaVaga());

        json.Should().Contain("\"isPcdFriendly\":");
        json.Should().Contain("\"companyId\":");
        json.Should().NotContain("\"CompanyId\":");
    }
}
