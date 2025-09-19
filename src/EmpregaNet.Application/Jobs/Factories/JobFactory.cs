using EmpregaNet.Application.Jobs.Commands;
using EmpregaNet.Domain.Entities;

namespace EmpregaNet.Application.Jobs.Factories;

public abstract class JobFactory
{
    public static Job Create(CreateJobCommand command, long companyId)
    {
        return new Job(
            title: command.Title,
            description: command.Description,
            salary: command.Salary,
            jobType: command.JobType,
            companyId: companyId
        );
    }

    public static Job Update(Job job, CreateJobCommand command)
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