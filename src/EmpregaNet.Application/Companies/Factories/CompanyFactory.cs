using EmpregaNet.Application.Companies.Command;
using EmpregaNet.Application.Jobs.Factories;
using EmpregaNet.Domain.Entities;

namespace EmpregaNet.Application.Companies.Factories;

public abstract record CompanyFactory
{

    /// <summary>
    /// Cria uma nova entidade Company sem associar vagas.
    /// </summary>
    public static Company Create(CreateCompanyCommand command)
    {
        var company = new Company(
            companyName: command.CompanyName,
            address: command.Address,
            registrationNumber: command.RegistrationNumber,
            email: command.Email,
            phone: command.Phone,
            typeOfActivity: command.TypeOfActivity
        );

        return company;
    }

    /// <summary>
    /// Atualiza uma entidade Company existente, incluindo a sincronização de vagas.
    /// </summary>
    public static Company Update(Company company, UpdateCompanyCommand command)
    {
        company.UpdateDetails(
            companyName: command.CompanyName,
            address: command.Address,
            registrationNumber: command.RegistrationNumber,
            email: command.Email,
            phone: command.Phone,
            typeOfActivity: command.TypeOfActivity
        );

        if (command.Jobs is null || !command.Jobs.Any())
        {
            company.ClearJobs();
        }
        else
        {
            var jobsToRemove = company.Jobs!
                .Where(j => !command.Jobs.Any(cj => cj.Id == j.Id))
                .ToList();

            foreach (var job in jobsToRemove)
            {
                company.RemoveJob(job);
            }

            foreach (var jobCommand in command.Jobs)
            {
                var existingJob = company.Jobs?.FirstOrDefault(j => j.Id == jobCommand.Id);

                if (existingJob is not null)
                {
                    JobFactory.Update(existingJob, jobCommand);
                }
                else
                {
                    var newJob = JobFactory.Create(jobCommand, company.Id);
                    company.AddJob(newJob);
                }
            }
        }
        
        return company;
    }
}