using FluentValidation;
using Microsoft.Extensions.Logging;
using EmpregaNet.Domain.Enums;
using EmpregaNet.Domain.Entities;
using EmpregaNet.Application.Common.Exceptions;
using EmpregaNet.Application.Common.Base;
using EmpregaNet.Application.Companies.Factories;
using EmpregaNet.Domain.Interfaces;
using EmpregaNet.Application.Utils.Helpers;
using System.ComponentModel.DataAnnotations;

namespace EmpregaNet.Application.Companies.Command;

public sealed record CreateCompanyCommand(
    string CompanyName,
    string Cnpj,
    string Email,
    string Phone,
    [EnumDataType(typeof(TypeOfActivityEnum))]
    string TypeOfActivity,
    Address Address
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
            var cnpjCleaned = request.entity.Cnpj.OnlyNumbers().Trim();
            var existingCompany = await _companyRepository.ExistsByCnpjAsync(cnpjCleaned);
            if (existingCompany)
            {
                _logger.LogWarning("Tentativa de criar empresa com CNPJ já existente: {CNPJ}", cnpjCleaned);
                throw new ValidationAppException(
                    nameof(cnpjCleaned),
                    $"Já existe uma empresa registrada com o CNPJ '{cnpjCleaned}'.",
                    DomainErrorEnum.RESOURCE_ALREADY_EXISTS);
            }

            var company = CompanyFactory.Create(request.entity);

            var createdCompanyId = await _companyRepository.CreateAsync(company, cancellationToken);
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
