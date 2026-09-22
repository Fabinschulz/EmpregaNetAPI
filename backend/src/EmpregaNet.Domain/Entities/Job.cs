using System.Diagnostics.CodeAnalysis;
using EmpregaNet.Domain.Common;
using EmpregaNet.Domain.Enums;
using EmpregaNet.Domain.Interfaces;

namespace EmpregaNet.Domain.Entities
{
    /// <summary>
    /// Vaga publicada por uma empresa.
    /// </summary>
    public class Job : BaseEntity, IAggregateRoot
    {
        /// <summary>Uma vaga sem posição nenhuma não é uma vaga.</summary>
        public const int MinPositions = 1;
        public const int MaxPositions = 999;

        private List<string> _requirements = [];
        private List<string> _benefits = [];
        public long CompanyId { get; private set; }
        public string Title { get; private set; }

        /// <summary>Chamada curta exibida no cartão do feed. O texto completo fica em <see cref="Description"/>.</summary>
        public string? Summary { get; private set; }
        public string Description { get; private set; }
        public decimal? SalaryMin { get; private set; }
        public decimal? SalaryMax { get; private set; }

        /// <summary><c>false</c> equivale a "a combinar": a vaga sai dos filtros por salário.</summary>
        public bool SalaryDisclosed { get; private set; }
        public JobTypeEnum JobType { get; private set; }
        public WorkModelEnum WorkModel { get; private set; }

        /// <summary>Turno ou escala. Critério de decisão de primeira ordem no público industrial.</summary>
        public WorkShiftEnum WorkShift { get; private set; }
        public ExperienceLevelEnum ExperienceLevel { get; private set; }
        public JobAreaEnum Area { get; private set; }
        public JobLocation Location { get; private set; }

        /// <summary>
        /// Vaga afirmativa para pessoas com deficiência.
        /// </summary>
        public bool IsPcdFriendly { get; private set; }

        /// <summary>
        /// Requisitos da vaga: escolaridade, CNH, NRs, operação de equipamento, sistemas.
        /// Persistido como <c>text[]</c> com índice GIN: o filtro usa sobreposição de arrays,
        /// sem join.
        /// </summary>
        public IReadOnlyList<string> Requirements => _requirements.AsReadOnly();
        public IReadOnlyList<string> Benefits => _benefits.AsReadOnly();
        public DateTimeOffset PublishedAt { get; private set; }
        public bool IsActive { get; private set; }

        /// <summary>Quantas pessoas a empresa quer contratar nesta vaga.</summary>
        public int Positions { get; private set; }

        /// <summary>
        /// Quantas vagas já estão ocupadas por candidatos aprovados.
        /// </summary>
        /// <remarks>
        /// Contador mantido no agregado, e não derivado de uma contagem de candidaturas. A regra
        /// "encerra ao encher" é uma invariante <b>da vaga</b>: mantê-la aqui deixa o próprio
        /// agregado recusar a posição que não tem, em vez de depender de quem lembrar de contar.
        /// O preço é existir um único ponto de mutação <see cref="FillPosition"/> e
        /// <see cref="ReleasePosition"/>, chamados pela transição de status da candidatura.
        /// </remarks>
        public int FilledPositions { get; private set; }

        /// <summary>Instante em que a vaga foi encerrada; <c>null</c> enquanto estiver activa.</summary>
        public DateTimeOffset? ClosedAt { get; private set; }

        /// <summary>
        /// O que encerrou a vaga; <c>null</c> enquanto estiver activa. É o que distingue
        /// "encerrada pela empresa" de "encerrada porque encheu".
        /// </summary>
        public JobClosureReasonEnum? ClosureReason { get; private set; }

        /// <summary>Posições ainda por preencher. Nunca negativo.</summary>
        public int AvailablePositions => Math.Max(0, Positions - FilledPositions);

        /// <summary>Situação legível da vaga: activa, encerrada pela empresa ou encerrada por preenchimento.</summary>
        public JobStatusEnum Status => JobStatus.Resolve(IsActive, ClosureReason);

        /// <summary>
        /// A vaga aceita novas candidaturas? Activa, não excluída e com posição sobrando.
        /// </summary>
        public bool IsOpenForApplications => IsActive && !IsDeleted && AvailablePositions > 0;

        /// <summary>Construtor de materialização do EF Core; não usar no domínio.</summary>
        private Job()
        {
            Title = null!;
            Description = null!;
            Location = null!;
        }

        public Job(
            long companyId,
            string title,
            string description,
            JobTypeEnum jobType,
            WorkModelEnum workModel,
            WorkShiftEnum workShift,
            ExperienceLevelEnum experienceLevel,
            JobAreaEnum area,
            JobLocation location,
            int positions = MinPositions,
            string? summary = null,
            decimal? salaryMin = null,
            decimal? salaryMax = null,
            bool salaryDisclosed = true,
            bool isPcdFriendly = false,
            IEnumerable<string>? requirements = null,
            IEnumerable<string>? benefits = null)
        {
            CompanyId = companyId;
            PublishedAt = DateTimeOffset.UtcNow;
            IsActive = true;
            FilledPositions = 0;

            ApplyDetails(
                title, description, jobType, workModel, workShift, experienceLevel, area, location, positions,
                summary, salaryMin, salaryMax, salaryDisclosed, isPcdFriendly, requirements, benefits);
        }

        public void UpdateJob(
            string title,
            string description,
            JobTypeEnum jobType,
            WorkModelEnum workModel,
            WorkShiftEnum workShift,
            ExperienceLevelEnum experienceLevel,
            JobAreaEnum area,
            JobLocation location,
            int positions = MinPositions,
            string? summary = null,
            decimal? salaryMin = null,
            decimal? salaryMax = null,
            bool salaryDisclosed = true,
            bool isPcdFriendly = false,
            IEnumerable<string>? requirements = null,
            IEnumerable<string>? benefits = null)
            => ApplyDetails(
                title, description, jobType, workModel, workShift, experienceLevel, area, location, positions,
                summary, salaryMin, salaryMax, salaryDisclosed, isPcdFriendly, requirements, benefits);

        /// <summary>
        /// Encerramento conduzido pelo recrutamento.
        /// </summary>
        /// <exception cref="InvalidOperationException">A vaga já está encerrada.</exception>
        public void Close()
        {
            if (!IsActive)
            {
                throw new InvalidOperationException("A vaga já está encerrada.");
            }

            CloseWith(JobClosureReasonEnum.Manual);
        }

        /// <summary>
        /// Ocupa uma posição com um candidato aprovado. Quando a última posição é preenchida, a
        /// vaga encerra-se sozinha com o motivo <see cref="JobClosureReasonEnum.Fulfilled"/>.
        /// </summary>
        /// <returns><c>true</c> se esta foi a posição que encerrou a vaga.</returns>
        /// <exception cref="InvalidOperationException">Vaga encerrada ou já sem posição livre.</exception>
        public bool FillPosition()
        {
            if (!IsActive)
            {
                throw new InvalidOperationException(
                    "Não é possível preencher uma posição de uma vaga encerrada.");
            }

            if (AvailablePositions == 0)
            {
                throw new InvalidOperationException(
                    "Todas as posições desta vaga já estão preenchidas.");
            }

            FilledPositions++;

            if (AvailablePositions > 0)
            {
                return false;
            }

            CloseWith(JobClosureReasonEnum.Fulfilled);
            return true;
        }

        /// <summary>
        /// Devolve ao total uma posição que estava ocupada, o candidato aprovado deixou de o ser.
        /// </summary>
        /// <exception cref="InvalidOperationException">Não há posição ocupada para libertar.</exception>
        public void ReleasePosition()
        {
            if (FilledPositions == 0)
            {
                throw new InvalidOperationException(
                    "Não há posição preenchida para liberar nesta vaga.");
            }

            FilledPositions--;
        }

        private void CloseWith(JobClosureReasonEnum reason)
        {
            IsActive = false;
            ClosedAt = DateTimeOffset.UtcNow;
            ClosureReason = reason;
        }

        /// <summary>
        /// Aplica os dados da vaga, validando o par salarial e normalizando as coleções.
        /// </summary>
        [MemberNotNull(nameof(Title), nameof(Description), nameof(Location))]
        private void ApplyDetails(
            string title,
            string description,
            JobTypeEnum jobType,
            WorkModelEnum workModel,
            WorkShiftEnum workShift,
            ExperienceLevelEnum experienceLevel,
            JobAreaEnum area,
            JobLocation location,
            int positions,
            string? summary,
            decimal? salaryMin,
            decimal? salaryMax,
            bool salaryDisclosed,
            bool isPcdFriendly,
            IEnumerable<string>? requirements,
            IEnumerable<string>? benefits)
        {
            Title = title;
            Description = description;
            JobType = jobType;
            WorkModel = workModel;
            WorkShift = workShift;
            ExperienceLevel = experienceLevel;
            Area = area;
            Location = location;
            Summary = summary;
            IsPcdFriendly = isPcdFriendly;

            ApplyPositions(positions);
            ApplySalary(salaryMin, salaryMax, salaryDisclosed);
            Replace(_requirements, requirements);
            Replace(_benefits, benefits);
        }

        /// <summary>
        /// O total de posições nunca pode descer abaixo do que já foi preenchido: isso tornaria
        /// <see cref="AvailablePositions"/> mentira e deixaria aprovados sem posição correspondente.
        /// </summary>
        private void ApplyPositions(int positions)
        {
            // Subir o total numa vaga encerrada produziria uma vaga encerrada **com posição livre**
            // estado que contradiz a própria razão do encerramento e que nada no sistema resolve,
            // porque encerramento não tem retorno. Editar o resto da vaga encerrada continua a valer.
            if (!IsActive && positions != Positions)
            {
                throw new InvalidOperationException(
                    "Não é possível alterar o total de posições de uma vaga encerrada.");
            }

            if (positions < MinPositions)
            {
                throw new InvalidOperationException(
                    $"A vaga precisa de pelo menos {MinPositions} posição.");
            }

            if (positions > MaxPositions)
            {
                throw new InvalidOperationException(
                    $"A vaga não pode ter mais de {MaxPositions} posições.");
            }

            if (positions < FilledPositions)
            {
                throw new InvalidOperationException(
                    $"Esta vaga já tem {FilledPositions} posição(ões) preenchida(s); " +
                    "o total não pode ficar abaixo desse número.");
            }

            Positions = positions;
        }

        private void ApplySalary(decimal? min, decimal? max, bool disclosed)
        {
            SalaryDisclosed = disclosed;

            if (!disclosed)
            {
                SalaryMin = null;
                SalaryMax = null;
                return;
            }

            if (min.HasValue && max.HasValue && min > max)
            {
                (min, max) = (max, min);
            }

            SalaryMin = min;
            SalaryMax = max;
        }

        /// <summary>
        /// Substitui o conteúdo da coleção no lugar, preservando a instância rastreada pelo EF Core.
        /// </summary>
        private static void Replace(List<string> target, IEnumerable<string>? values)
        {
            target.Clear();

            if (values is null)
            {
                return;
            }

            target.AddRange(values
                .Where(v => !string.IsNullOrWhiteSpace(v))
                .Select(v => v.Trim())
                .DistinctBy(v => v.ToLowerInvariant()));
        }
    }
}
