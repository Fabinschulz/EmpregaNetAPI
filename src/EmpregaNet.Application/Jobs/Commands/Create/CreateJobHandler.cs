using FluentValidation;
using Microsoft.Extensions.Logging;
using EmpregaNet.Domain.Enums;
using EmpregaNet.Application.Common.Exceptions;
using EmpregaNet.Application.Common.Base;
using EmpregaNet.Domain.Interfaces;
using System.ComponentModel.DataAnnotations;
using EmpregaNet.Application.Jobs.Factories;

namespace EmpregaNet.Application.Jobs.Commands;

public sealed record CreateJobCommand(
    long CompanyId,
    string Title,
    string Description,
    [EnumDataType(typeof(JobTypeEnum))]
    string JobType,
    decimal Salary
) : IJobCommand;

public sealed class CreateJobHandler : IRequestHandler<CreateCommand<CreateJobCommand>, long>
{
    private readonly IJobRepository _jobRepository;
    private readonly ICompanyRepository _companyRepository;
    private readonly IValidator<CreateCommand<CreateJobCommand>> _validator;
    private readonly ILogger<CreateJobHandler> _logger;
    public CreateJobHandler(IJobRepository jobRepository,
                            ICompanyRepository companyRepository,
                            IValidator<CreateCommand<CreateJobCommand>> validator,
                            ILogger<CreateJobHandler> logger)
    {
        _jobRepository = jobRepository;
        _companyRepository = companyRepository;
        _validator = validator;
        _logger = logger;
    }
    public async Task<long> Handle(CreateCommand<CreateJobCommand> request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Iniciando a criação de uma nova vaga de emprego.");
        try
        {
            var companyExists = await _companyRepository.GetByIdAsync(request.entity.CompanyId);
            if (companyExists is null)
            {
                _logger.LogError("Empresa não encontrada com ID: {CompanyId}", request.entity.CompanyId);
                throw new ValidationAppException(
                    nameof(request.entity.CompanyId),
                    $"Empresa com ID '{request.entity.CompanyId}' não encontrada.",
                    DomainErrorEnum.RESOURCE_ID_NOT_FOUND);
            }

            var job = JobFactory.Create(request.entity);
            var createdJobId = await _jobRepository.CreateAsync(job, cancellationToken);
            _logger.LogInformation("Vaga de emprego criada com sucesso. ID: {JobId}", createdJobId);
            return createdJobId.Id;

        }
        catch (ValidationAppException ex)
        {
            _logger.LogWarning("Validação falhou ao criar vaga de emprego: {Message}", ex.Message);
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro inesperado ao criar vaga de emprego: {Message}", ex.Message);
            throw;
        }

    }

}
