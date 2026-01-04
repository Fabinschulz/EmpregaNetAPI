using FluentValidation;
using Microsoft.Extensions.Logging;
using EmpregaNet.Domain.Enums;
using EmpregaNet.Application.Jobs.ViewModel;
using System.ComponentModel.DataAnnotations;
using EmpregaNet.Domain.Entities;
using EmpregaNet.Application.Common.Exceptions;
using EmpregaNet.Application.Companies.ViewModel;
using EmpregaNet.Application.Common.Base;
using EmpregaNet.Application.Companies.Factories;
using EmpregaNet.Domain.Interfaces;

namespace EmpregaNet.Application.Companies.Command
{
    public sealed record UpdateCompanyCommand(
        string CompanyName,
        string Cnpj,
        string Email,
        string Phone,
        [EnumDataType(typeof(TypeOfActivityEnum))]
        string TypeOfActivity,
        Address Address
    ) : ICompanyCommand;

    public sealed class UpdateCompanyHandler : IRequestHandler<UpdateCommand<UpdateCompanyCommand, CompanyViewModel>, CompanyViewModel>
    {
        private readonly ICompanyRepository _companyRepository;
        private readonly IValidator<UpdateCommand<UpdateCompanyCommand, CompanyViewModel>> _validator;
        private readonly ILogger<UpdateCompanyHandler> _logger;

        public UpdateCompanyHandler(ICompanyRepository companyRepository,
                                    IValidator<UpdateCommand<UpdateCompanyCommand, CompanyViewModel>> validator,
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
                var company = await _companyRepository.GetByIdAsync(request.Id);

                if (company is null)
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

                var updatedCompany = CompanyFactory.Update(company, request.entity);
                await _companyRepository.UpdateAsync(updatedCompany, cancellationToken);

                return updatedCompany.ToViewModel();
            }
            catch (ValidationAppException ex)
            {
                _logger.LogWarning(ex, "Falha de validação ao atualizar empresa: {Message}. Request: {@Request}", ex.Message, request);
                throw;
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Erro de lógica de negócio ao atualizar empresa: {Message}. Request: {@Request}", ex.Message, request);
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro inesperado ao atualizar empresa (ID: {Id}). Request: {@Request}", request.Id, request);
                throw new Exception("Ocorreu um erro inesperado ao atualizar a empresa. Por favor, tente novamente mais tarde.");
            }
        }
    }
}