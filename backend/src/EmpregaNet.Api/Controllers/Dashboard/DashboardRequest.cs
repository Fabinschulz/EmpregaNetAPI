using EmpregaNet.Application.Dashboard;
using EmpregaNet.Domain.Common.Dashboard;
using EmpregaNet.Domain.Enums;
using Microsoft.AspNetCore.Mvc;

namespace EmpregaNet.Api.Controllers.Dashboard;

public class DashboardRequest
{
    /// <summary>Janela de análise. <c>Custom</c> exige <c>from</c> e <c>to</c>.</summary>
    [FromQuery(Name = "period")]
    public DashboardPeriodEnum Period { get; set; } = DashboardPeriodEnum.Last30Days;

    /// <summary>Primeiro dia do período personalizado (<c>yyyy-MM-dd</c>), em horário de Brasília.</summary>
    [FromQuery(Name = "from")]
    public DateOnly? From { get; set; }

    /// <summary>Último dia do período personalizado, inclusivo (<c>yyyy-MM-dd</c>).</summary>
    [FromQuery(Name = "to")]
    public DateOnly? To { get; set; }

    /// <summary>
    /// Empresa a analisar. Apenas administrador pode escolher; recrutador e gestor recebem sempre a
    /// empresa a que estão vinculados, e pedir outra é recusado.
    /// </summary>
    [FromQuery(Name = "companyId")]
    public long? CompanyId { get; set; }

    /// <summary>UFs a considerar. Vazio significa todas.</summary>
    [FromQuery(Name = "state")]
    public UF[]? States { get; set; }

    /// <summary>Áreas profissionais a considerar. Vazio significa todas.</summary>
    [FromQuery(Name = "area")]
    public JobAreaEnum[]? Areas { get; set; }

    /// <summary>
    /// Status da candidatura. Recorta <b>apenas</b> números de candidatura, não filtra contagens de
    /// vagas, utilizadores nem empresas, e não se aplica à distribuição por status nem ao funil.
    /// </summary>
    [FromQuery(Name = "status")]
    public ApplicationStatusEnum? ApplicationStatus { get; set; }

    public DashboardFilterInput ToFilter() => new(
        Period: Period,
        From: From,
        To: To,
        CompanyId: CompanyId,
        States: States,
        Areas: Areas,
        ApplicationStatus: ApplicationStatus);
}

/// <summary>Recortes do dashboard mais a granularidade das séries temporais.</summary>
public sealed class DashboardTrendsRequest : DashboardRequest
{
    /// <summary>
    /// Granularidade dos baldes. Omitido, o servidor escolhe pelo tamanho da janela: diário até
    /// 31 dias, semanal até 120, mensal acima disso.
    /// </summary>
    [FromQuery(Name = "granularity")]
    public DashboardGranularityEnum? Granularity { get; set; }
}

/// <summary>Recortes do dashboard mais os parâmetros do ranking de vagas.</summary>
public sealed class DashboardJobsRequest : DashboardRequest
{
    /// <summary>Critério do ranking.</summary>
    [FromQuery(Name = "ranking")]
    public DashboardJobRankingEnum Ranking { get; set; } = DashboardJobRankingEnum.MostApplications;

    /// <summary>Linhas devolvidas. Máximo 50.</summary>
    [FromQuery(Name = "limit")]
    public int Limit { get; set; } = 8;

    /// <summary>
    /// Restringe a vagas abertas. Falso inclui encerradas, útil para análise histórica.
    /// </summary>
    [FromQuery(Name = "onlyActive")]
    public bool OnlyActive { get; set; } = true;
}


