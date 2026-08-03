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

            ApplyDetails(
                title, description, jobType, workModel, workShift, experienceLevel, area, location,
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
            string? summary = null,
            decimal? salaryMin = null,
            decimal? salaryMax = null,
            bool salaryDisclosed = true,
            bool isPcdFriendly = false,
            IEnumerable<string>? requirements = null,
            IEnumerable<string>? benefits = null)
            => ApplyDetails(
                title, description, jobType, workModel, workShift, experienceLevel, area, location,
                summary, salaryMin, salaryMax, salaryDisclosed, isPcdFriendly, requirements, benefits);

        public void Close() => IsActive = false;

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

            ApplySalary(salaryMin, salaryMax, salaryDisclosed);
            Replace(_requirements, requirements);
            Replace(_benefits, benefits);
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
