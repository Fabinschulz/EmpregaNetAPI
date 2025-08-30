using EmpregaNet.Application.Common.Command;
using EmpregaNet.Application.Companies.Command;
using Mediator.Interfaces;
using EmpregaNet.Domain.Interfaces;
using FluentValidation;
using Microsoft.Extensions.Logging;
using Common.Exceptions;
using EmpregaNet.Domain.Enums;
using EmpregaNet.Application.Companies.Factories;

namespace EmpregaNet.Application.Handler.Companies.Command;

public sealed class CreateCompanyCommandHandler : IRequestHandler<CreateCommand<CompanyCommand>, long>
{
    private readonly ICompanyRepository _companyRepository;
    private readonly IValidator<CreateCommand<CompanyCommand>> _validator;
    private readonly ILogger<CreateCompanyCommandHandler> _logger;

    public CreateCompanyCommandHandler(ICompanyRepository companyRepository,
                                       IValidator<CreateCommand<CompanyCommand>> validator,
                                       ILogger<CreateCompanyCommandHandler> logger)
    {
        _companyRepository = companyRepository;
        _validator = validator;
        _logger = logger;
    }
    public async Task<long> Handle(CreateCommand<CompanyCommand> request, CancellationToken cancellationToken)
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
