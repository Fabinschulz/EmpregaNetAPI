using FluentValidation;
using Microsoft.Extensions.Logging;
using EmpregaNet.Domain.Enums;
using System.ComponentModel.DataAnnotations;
using EmpregaNet.Domain.Entities;
using EmpregaNet.Application.Jobs.Commands;
using EmpregaNet.Application.Common.Exceptions;
using EmpregaNet.Application.Common.Base;
using EmpregaNet.Application.Companies.Factories;
using EmpregaNet.Domain.Interfaces;

namespace EmpregaNet.Application.Companies.Command;

public sealed record CreateCompanyCommand(
    string CompanyName,
    string RegistrationNumber,
    string Email,
    string Phone,
    [EnumDataType(typeof(TypeOfActivityEnum))]
    TypeOfActivityEnum TypeOfActivity,
    Address Address,
    ICollection<CreateJobCommand>? Jobs = null
) : ICompanyCommand;

public sealed class CreateCompanyCommandHandler : IRequestHandler<CreateCommand<CreateCompanyCommand>, long>
{
    private readonly ICompanyRepository _companyRepository;
    private readonly IValidator<CreateCommand<CreateCompanyCommand>> _validator;
    private readonly ILogger<CreateCompanyCommandHandler> _logger;

    public CreateCompanyCommandHandler(ICompanyRepository companyRepository,
                                       IValidator<CreateCommand<CreateCompanyCommand>> validator,
                                       ILogger<CreateCompanyCommandHandler> logger)
    {
        _companyRepository = companyRepository;
        _validator = validator;
        _logger = logger;
    }
    public async Task<long> Handle(CreateCommand<CreateCompanyCommand> request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Iniciando o processo de criação da empresa: {CompanyName}", request.entity.CompanyName);

        try
        {

            var existingCompany = await _companyRepository.GetByRegistrationNumberAsync(request.entity.RegistrationNumber);
            if (existingCompany is not null)
            {
                _logger.LogWarning("Tentativa de criar empresa com registro já existente: {RegistrationNumber}", request.entity.RegistrationNumber);
                throw new ValidationAppException(
                    nameof(request.entity.RegistrationNumber),
                    $"Já existe uma empresa registrada com o número de registro '{request.entity.RegistrationNumber}'.",
                    DomainErrorEnum.RESOURCE_ALREADY_EXISTS);
            }

            var company = CompanyFactory.Create(request.entity);

            var createdCompanyId = await _companyRepository.CreateAsync(company);
            _logger.LogInformation("Empresa criada com sucesso. ID: {CompanyId}", createdCompanyId);

            return createdCompanyId.Id;

        }
        catch (ValidationAppException ex)
        {
            _logger.LogWarning(ex, "Falha de validação ao criar empresa. Request: {@Request}", request);
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro inesperado ao criar empresa. Request: {@Request}", request);
            throw new Exception("Ocorreu um erro inesperado ao criar a empresa. Por favor, tente novamente mais tarde.");
        }

    }

}
