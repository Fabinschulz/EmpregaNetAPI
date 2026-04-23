using EmpregaNet.Application.Jobs.Commands;
using EmpregaNet.Domain.Entities;
using EmpregaNet.Domain.Enums;

namespace EmpregaNet.Application.Jobs.Factories;

public abstract class JobFactory
{
    public static Job Create(CreateJobCommand command)
    {
        return new Job(
            title: command.Title,
            description: command.Description,
            salary: command.Salary,
            jobType: Enum.Parse<JobTypeEnum>(command.JobType),
            companyId: command.CompanyId
        );
    }

    public static Job Update(Job job, UpdateJobCommand command)
    {
        job.UpdateJob(
            title: command.Title,
            description: command.Description,
            salary: command.Salary,
            jobType: Enum.Parse<JobTypeEnum>(command.JobType)
        );
        return job;
    }
}