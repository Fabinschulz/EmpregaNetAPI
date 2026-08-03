namespace EmpregaNet.Application.Jobs;

/// <summary>
/// Vocabulário curado de requisitos e benefícios das vagas.
/// </summary>
public static class JobVocabulary
{
    /// <summary>Teto de itens aceites por vaga, evita listas infladas que poluem a UI e o feed.</summary>
    public const int MaxItemsPerJob = 20;

    /// <summary>
    /// Requisitos agrupados por natureza. O agrupamento é contrato de UI: o formulário e o painel
    /// de filtros exibem por grupo, porque uma lista corrida de 60 itens não se navega.
    /// </summary>
    public static readonly IReadOnlyList<JobVocabularyGroup> RequirementGroups =
    [
        new("Escolaridade",
        [
            "Ensino Fundamental incompleto",
            "Ensino Fundamental completo",
            "Ensino Médio incompleto",
            "Ensino Médio completo",
            "Curso Técnico",
            "Ensino Superior incompleto",
            "Ensino Superior completo"
        ]),

        new("CNH",
        [
            "CNH A", "CNH B", "CNH C", "CNH D", "CNH E", "Veículo próprio"
        ]),

        new("Normas Regulamentadoras",
        [
            "NR-10 (elétrica)",
            "NR-11 (movimentação de cargas)",
            "NR-12 (máquinas)",
            "NR-20 (inflamáveis)",
            "NR-33 (espaço confinado)",
            "NR-35 (trabalho em altura)"
        ]),

        new("Operação de equipamentos",
        [
            "Empilhadeira",
            "Paleteira elétrica",
            "Ponte rolante",
            "Rebocador",
            "Retroescavadeira",
            "Torno mecânico",
            "Solda",
            "CLP / automação",
            "Injetora",
            "Prensa",
            "Fresadora",
            "Torno CNC",
            "Máquina de corte",
            "Máquina de dobra",
            "Máquina de solda",
            "Máquina de pintura",
        ]),

        new("Sistemas e informática",
        [
            "Coletor de dados",
            "WMS",
            "SAP",
            "TOTVS",
            "Excel básico",
            "Excel avançado",
            "Pacote Office",
            "Power BI"
        ]),

        new("Idiomas",
        [
            "Inglês básico", "Inglês intermediário", "Inglês avançado", "Espanhol"
        ]),

        new("Outros",
        [
            "Disponibilidade para viagens",
            "Disponibilidade para turnos",
            "Experiência com liderança de equipe",
            "Conhecimento em 5S",
            "Conhecimento em Lean / Kaizen",
            "Boas práticas de fabricação (BPF)"
        ])
    ];

    /// <summary>
    /// Benefícios agrupados. <b>Fretado</b>, <b>cesta básica</b> e os adicionais legais
    /// (noturno, insalubridade, periculosidade) são os mais relevantes para o público industrial, mas a UI exibe todos os grupos
    /// para não rejeitar vagas que ofereçam benefícios menos comuns.
    /// </summary>
    public static readonly IReadOnlyList<JobVocabularyGroup> BenefitGroups =
    [
        new("Transporte e alimentação",
        [
            "Fretado",
            "Vale Transporte",
            "Refeitório no local",
            "Vale Refeição",
            "Vale Alimentação",
            "Cesta Básica"
        ]),

        new("Saúde e bem-estar",
        [
            "Plano de Saúde",
            "Plano Odontológico",
            "Seguro de Vida",
            "Convênio Farmácia",
            "Auxílio Creche"
        ]),

        new("Remuneração adicional",
        [
            "Participação nos Lucros (PLR)",
            "Adicional Noturno",
            "Adicional de Insalubridade",
            "Adicional de Periculosidade",
            "Prêmio por assiduidade",
            "Hora extra"
        ]),

        new("Desenvolvimento e jornada",
        [
            "Bolsa de Estudos",
            "Convênio SENAI / SESI",
            "Plano de carreira",
            "Horário Flexível",
            "Previdência Privada"
        ])
    ];

    public static readonly IReadOnlyList<string> Requirements =
        RequirementGroups.SelectMany(g => g.Items).ToList();

    public static readonly IReadOnlyList<string> Benefits =
        BenefitGroups.SelectMany(g => g.Items).ToList();

    private static readonly HashSet<string> RequirementSet =
        new(Requirements, StringComparer.OrdinalIgnoreCase);

    private static readonly HashSet<string> BenefitSet =
        new(Benefits, StringComparer.OrdinalIgnoreCase);

    public static bool IsKnownRequirement(string value) => RequirementSet.Contains(value.Trim());

    public static bool IsKnownBenefit(string value) => BenefitSet.Contains(value.Trim());

    /// <summary>
    /// Devolve o rótulo canónico do vocabulário para o valor informado, preservando a grafia
    /// oficial ("cnh d" -> "CNH D"). Valor desconhecido volta apenas aparado, rejeitar é decisão
    /// do validator, não da normalização.
    /// </summary>
    public static string CanonicalRequirement(string value) => Canonical(value, Requirements, RequirementSet);

    /// <inheritdoc cref="CanonicalRequirement"/>
    public static string CanonicalBenefit(string value) => Canonical(value, Benefits, BenefitSet);

    private static string Canonical(string value, IReadOnlyList<string> source, HashSet<string> lookup)
    {
        var trimmed = value.Trim();

        if (!lookup.Contains(trimmed))
        {
            return trimmed;
        }

        return source.First(item => string.Equals(item, trimmed, StringComparison.OrdinalIgnoreCase));
    }
}

/// <summary>Grupo nomeado de itens do vocabulário, na ordem em que a UI deve exibi-los.</summary>
public sealed record JobVocabularyGroup(string Label, IReadOnlyList<string> Items);
