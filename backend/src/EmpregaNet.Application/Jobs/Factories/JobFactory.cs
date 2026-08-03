using EmpregaNet.Application.Jobs.Commands;
using EmpregaNet.Domain.Entities;
using EmpregaNet.Domain.Enums;

namespace EmpregaNet.Application.Jobs.Factories;

public abstract class JobFactory
{
    public static Job Create(CreateJobCommand command)
    {
        return new Job(
            companyId: command.CompanyId,
            title: command.Title,
            description: command.Description,
            jobType: Parse<JobTypeEnum>(command.JobType),
            workModel: Parse<WorkModelEnum>(command.WorkModel),
            workShift: Parse<WorkShiftEnum>(command.WorkShift),
            experienceLevel: Parse<ExperienceLevelEnum>(command.ExperienceLevel),
            area: Parse<JobAreaEnum>(command.Area),
            location: BuildLocation(command),
            summary: command.Summary,
            salaryMin: command.SalaryMin,
            salaryMax: command.SalaryMax,
            salaryDisclosed: command.SalaryDisclosed,
            isPcdFriendly: command.IsPcdFriendly,
            requirements: CanonicalRequirements(command.Requirements),
            benefits: CanonicalBenefits(command.Benefits)
        );
    }

    public static Job Update(Job job, UpdateJobCommand command)
    {
        job.UpdateJob(
            title: command.Title,
            description: command.Description,
            jobType: Parse<JobTypeEnum>(command.JobType),
            workModel: Parse<WorkModelEnum>(command.WorkModel),
            workShift: Parse<WorkShiftEnum>(command.WorkShift),
            experienceLevel: Parse<ExperienceLevelEnum>(command.ExperienceLevel),
            area: Parse<JobAreaEnum>(command.Area),
            location: BuildLocation(command),
            summary: command.Summary,
            salaryMin: command.SalaryMin,
            salaryMax: command.SalaryMax,
            salaryDisclosed: command.SalaryDisclosed,
            isPcdFriendly: command.IsPcdFriendly,
            requirements: CanonicalRequirements(command.Requirements),
            benefits: CanonicalBenefits(command.Benefits)
        );
        return job;
    }

    private static TEnum Parse<TEnum>(string value) where TEnum : struct, Enum
        => Enum.Parse<TEnum>(value, ignoreCase: true);

    private static JobLocation BuildLocation(IJobCommand command) => new()
    {
        City = command.City.Trim(),
        State = Parse<UF>(command.State),
        Country = "BR"
    };

    /// <summary>
    /// Grava a grafia canónica do vocabulário. Sem isto, "cnh d" e "CNH D" viram dois valores
    /// distintos no <c>text[]</c> e o filtro do feed encontra só um deles.
    /// </summary>
    private static IEnumerable<string>? CanonicalRequirements(IReadOnlyList<string>? values)
        => values?.Select(JobVocabulary.CanonicalRequirement);

    /// <inheritdoc cref="CanonicalRequirements"/>
    private static IEnumerable<string>? CanonicalBenefits(IReadOnlyList<string>? values)
        => values?.Select(JobVocabulary.CanonicalBenefit);
}
