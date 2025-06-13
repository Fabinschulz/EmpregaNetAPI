using EmpregaNet.Application.Common.Command;
using EmpregaNet.Application.Companies.Command;
using EmpregaNet.Application.Messages;
using Mediator.Interfaces;
using EmpregaNet.Domain.Entities;
using EmpregaNet.Domain.Interfaces;
using FluentValidation;
using Microsoft.Extensions.Logging;
using Common.Exceptions;
using EmpregaNet.Domain.Enums;

namespace EmpregaNet.Application.Handler.Companies.Command;

public sealed class CreateCompanyCommandHandler : IRequestHandler<CreateCommand<CreateCompanyCommand>, long>
{
    private readonly ICompanyRepository _companyRepository;
    private readonly IValidator<CreateCompanyCommand> _validator;
    private readonly ILogger<CreateCompanyCommandHandler> _logger;
    private readonly IMediator _mediator;

    public CreateCompanyCommandHandler(ICompanyRepository companyRepository,
                                       IValidator<CreateCompanyCommand> validator,
                                       ILogger<CreateCompanyCommandHandler> logger,
                                       IMediator mediator)
    {
        _companyRepository = companyRepository;
        _validator = validator;
        _logger = logger;
        _mediator = mediator;
    }
    public async Task<long> Handle(CreateCommand<CreateCompanyCommand> request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Iniciando o processo de criação da empresa: {CompanyName}", request.entity.CompanyName);

        try
        {
            var validationResult = await _validator.ValidateAsync(request.entity, cancellationToken);
            if (!validationResult.IsValid)
            {
                _logger.LogWarning("Falha na validação da criação da empresa: {Errors}", string.Join(", ", validationResult.Errors.Select(e => e.ErrorMessage)));
                throw new ValidationAppException(validationResult.Errors);
            }

            var existingCompany = await _companyRepository.GetByRegistrationNumberAsync(request.entity.RegistrationNumber);
            if (existingCompany != null)
            {
                _logger.LogWarning("Tentativa de criar empresa com registro já existente: {RegistrationNumber}", request.entity.RegistrationNumber);
                throw new ValidationAppException(
                    nameof(request.entity.RegistrationNumber),
                    $"Já existe uma empresa registrada com o número de registro '{request.entity.RegistrationNumber}'.",
                    DomainErrorEnum.RESOURCE_ALREADY_EXISTS);
            }

            var company = new Company
            {
                TypeOfActivity = request.entity.TypeOfActivity,
                CompanyName = request.entity.CompanyName,
                Address = request.entity.Address,
                RegistrationNumber = request.entity.RegistrationNumber,
                Email = request.entity.Email,
                Phone = request.entity.Phone
            };

            var createdCompanyId = await _companyRepository.CreateAsync(company);
            _logger.LogInformation("Empresa criada com sucesso. ID: {CompanyId}", createdCompanyId);
            await _mediator.Publish(new EntityEvent<Company>(company), cancellationToken);

            return createdCompanyId.Id;

        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Erro de lógica de negócio ao criar empresa: {Message}. Request: {@Request}", ex.Message, request);
            throw;
        }
        catch (KeyNotFoundException ex)
        {
            _logger.LogWarning(ex, "Entidade não encontrada ao criar empresa: {Message}. Request: {@Request}", ex.Message, request);
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro inesperado ao criar empresa. Request: {@Request}", request);
            throw new Exception("Ocorreu um erro inesperado ao criar a empresa. Por favor, tente novamente mais tarde.");
        }

    }

}
