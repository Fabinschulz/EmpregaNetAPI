using EmpregaNet.Application.Jobs.Queries;
using EmpregaNet.Domain.Enums;
using Microsoft.AspNetCore.Mvc;

namespace EmpregaNet.Api.Controllers.Jobs;

/// <summary>
/// Parâmetros de consulta do feed de vagas.
/// </summary>
/// <remarks>
/// Filtros de múltipla escolha repetem a chave no singular
/// (<c>?shift=SegundoTurno&amp;shift=TerceiroTurno</c>) mais legível na URL compartilhada, que é
/// o mecanismo de persistência de estado do feed.
///
/// <para>Objeto de binding em vez de dezenas de parâmetros soltos na action: mantém o controller
/// fino e a documentação do Swagger legível.</para>
/// </remarks>
public sealed class JobsFeedRequest
{
    /// <summary>Página, começando em 1.</summary>
    [FromQuery(Name = "page")]
    public int Page { get; set; } = 1;

    /// <summary>Itens por página. Máximo 50.</summary>
    [FromQuery(Name = "size")]
    public int Size { get; set; } = 20;

    /// <summary>Busca livre por cargo, empresa, requisito, benefício ou texto da vaga.</summary>
    [FromQuery(Name = "search")]
    public string? Search { get; set; }

    [FromQuery(Name = "city")]
    public string[]? Cities { get; set; }

    [FromQuery(Name = "state")]
    public UF[]? States { get; set; }

    [FromQuery(Name = "workModel")]
    public WorkModelEnum[]? WorkModels { get; set; }

    /// <summary>Turno ou escala.</summary>
    [FromQuery(Name = "shift")]
    public WorkShiftEnum[]? WorkShifts { get; set; }

    [FromQuery(Name = "jobType")]
    public JobTypeEnum[]? JobTypes { get; set; }

    /// <summary>Experiência exigida, em tempo.</summary>
    [FromQuery(Name = "experience")]
    public ExperienceLevelEnum[]? ExperienceLevels { get; set; }

    [FromQuery(Name = "area")]
    public JobAreaEnum[]? Areas { get; set; }

    [FromQuery(Name = "requirement")]
    public string[]? Requirements { get; set; }

    [FromQuery(Name = "benefit")]
    public string[]? Benefits { get; set; }

    [FromQuery(Name = "companyId")]
    public long[]? CompanyIds { get; set; }

    /// <summary>Piso desejado. A vaga entra quando a faixa dela cruza o intervalo pedido.</summary>
    [FromQuery(Name = "salaryMin")]
    public decimal? SalaryMin { get; set; }

    /// <summary>Teto desejado. Vagas com salário a combinar saem quando há qualquer limite.</summary>
    [FromQuery(Name = "salaryMax")]
    public decimal? SalaryMax { get; set; }

    /// <summary>Restringe a vagas afirmativas para PcD. Falso não exclui essas vagas.</summary>
    [FromQuery(Name = "pcd")]
    public bool OnlyPcdFriendly { get; set; }

    /// <summary>Janela de publicação. <c>Today</c> conta do início do dia; <c>Last24Hours</c>, de agora.</summary>
    [FromQuery(Name = "publishedWithin")]
    public JobPublishedWindowEnum PublishedWithin { get; set; } = JobPublishedWindowEnum.Any;

    /// <summary>Ordenação. <c>Relevance</c> sem busca ativa cai para <c>Recent</c>.</summary>
    [FromQuery(Name = "sort")]
    public JobFeedSortEnum Sort { get; set; } = JobFeedSortEnum.Recent;

    public GetJobsFeedQuery ToQuery() => new(
        Page: Page,
        Size: Size,
        Search: Search,
        Cities: Cities,
        States: States,
        WorkModels: WorkModels,
        WorkShifts: WorkShifts,
        JobTypes: JobTypes,
        ExperienceLevels: ExperienceLevels,
        Areas: Areas,
        Requirements: Requirements,
        Benefits: Benefits,
        CompanyIds: CompanyIds,
        SalaryMin: SalaryMin,
        SalaryMax: SalaryMax,
        OnlyPcdFriendly: OnlyPcdFriendly,
        PublishedWithin: PublishedWithin,
        Sort: Sort);
}
