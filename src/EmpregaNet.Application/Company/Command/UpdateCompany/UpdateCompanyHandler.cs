using EmpregaNet.Application.Common.Command;
using EmpregaNet.Application.Companies.ViewModel;
using EmpregaNet.Application.Jobs.ViewModel;
using Mediator.Interfaces;
using EmpregaNet.Domain.Entities;
using EmpregaNet.Domain.Interfaces;
using FluentValidation;
using Microsoft.Extensions.Logging;
using Common.Exceptions;
using EmpregaNet.Domain.Enums;

namespace EmpregaNet.Application.Companies.Command;

public sealed class UpdateCompanyHandler : IRequestHandler<UpdateCommand<UpdateCompanyCommand, CompanyViewModel>, CompanyViewModel>
{
    private readonly ICompanyRepository _companyRepository;
    private readonly IValidator<UpdateCompanyCommand> _validator;
    private readonly ILogger<UpdateCompanyHandler> _logger;

    public UpdateCompanyHandler(ICompanyRepository companyRepository,
                                IValidator<UpdateCompanyCommand> validator,
                                ILogger<UpdateCompanyHandler> logger)
    {
        _companyRepository = companyRepository;
        _validator = validator;
        _logger = logger;
    }

    public async Task<CompanyViewModel> Handle(UpdateCommand<UpdateCompanyCommand, CompanyViewModel> request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Iniciando o processo de atualização da empresa: {CompanyId}", request.Id);

        try
        {
            var validationResult = await _validator.ValidateAsync(request.entity, cancellationToken);
            if (!validationResult.IsValid)
            {
                _logger.LogWarning("Falha na validação da atualização da Empresa: {Errors}", string.Join(", ", validationResult.Errors.Select(e => e.ErrorMessage)));
                throw new ValidationAppException(validationResult.Errors);
            }

            var company = await _companyRepository.GetByIdAsync(request.Id);
            if (company == null)
            {
                _logger.LogError("Empresa não encontrada com ID: {CompanyId}", request.Id);
                throw new ValidationAppException(
                    nameof(request.Id),
                    $"Empresa com ID '{request.Id}' não encontrada.",
                    DomainErrorEnum.RESOURCE_ID_NOT_FOUND);
            }

            if (company.IsDeleted)
            {
                _logger.LogError("Tentativa de atualização de empresa excluída. ID: {CompanyId}", request.Id);
                throw new ValidationAppException(
                    nameof(request.Id),
                    $"Não é possível atualizar uma empresa excluída. ID '{request.Id}' já está marcado como excluído.",
                    DomainErrorEnum.INVALID_ACTION_FOR_STATUS);
            }

            if (request.entity.TypeOfActivity.HasValue) company.TypeOfActivity = request.entity.TypeOfActivity;
            if (!string.IsNullOrEmpty(request.entity.CompanyName)) company.CompanyName = request.entity.CompanyName;
            if (request.entity.Address != null) company.Address = request.entity.Address;
            if (!string.IsNullOrEmpty(request.entity.Email)) company.Email = request.entity.Email;
            if (!string.IsNullOrEmpty(request.entity.Phone)) company.Phone = request.entity.Phone;

            await _companyRepository.UpdateAsync(company);
            _logger.LogInformation("Empresa atualizada com sucesso. ID: {CompanyId}", company.Id);

            return new CompanyViewModel
            {
                Id = company.Id,
                TypeOfActivity = company.TypeOfActivity,
                CompanyName = company.CompanyName,
                Address = company.Address,
                RegistrationNumber = company.RegistrationNumber,
                Email = company.Email,
                Phone = company.Phone,
                Jobs = (company.Jobs ?? Enumerable.Empty<Job>()).Select(j => new JobViewModel
                {
                    Id = j.Id,
                    Title = j.Title,
                    Description = j.Description,
                    Salary = j.Salary,
                    CreatedAt = j.CreatedAt,
                    UpdatedAt = j.UpdatedAt,
                    IsDeleted = j.IsDeleted
                }).ToList(),
            };
        }
        catch (KeyNotFoundException ex)
        {
            _logger.LogWarning(ex, "Funcionalidade ou Ações não encontradas para atualização: {Message}. Request: {@Request}", ex.Message, request);
            throw;
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Erro de lógica de negócio ao atualizar funcionalidade: {Message}. Request: {@Request}", ex.Message, request);
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro inesperado ao atualizar funcionalidade (ID: {Id}). Request: {@Request}", request.Id, request);
            throw new Exception("Ocorreu um erro inesperado ao atualizar a funcionalidade. Por favor, tente novamente mais tarde.");
        }
    }
}
