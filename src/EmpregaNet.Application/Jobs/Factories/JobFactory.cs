using EmpregaNet.Application.Jobs.Commands;
using EmpregaNet.Domain.Entities;

namespace EmpregaNet.Application.Jobs.Factories;

public abstract class JobFactory
{
    public static Job Create(JobCommand command, long companyId)
    {
        return new Job(
            title: command.Title,
            description: command.Description,
            salary: command.Salary,
            jobType: command.JobType,
            companyId: companyId
        );
    }

    public static Job Update(Job job, JobCommand command)
    {
        job.UpdateDetails(
            title: command.Title,
            description: command.Description,
            salary: command.Salary,
            jobType: command.JobType
        );
        return job;
    }
}