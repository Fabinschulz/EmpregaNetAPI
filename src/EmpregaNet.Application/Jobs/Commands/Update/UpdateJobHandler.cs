using FluentValidation;
using Microsoft.Extensions.Logging;
using EmpregaNet.Domain.Enums;
using EmpregaNet.Application.Jobs.ViewModel;
using System.ComponentModel.DataAnnotations;
using EmpregaNet.Application.Common.Exceptions;
using EmpregaNet.Application.Companies.ViewModel;
using EmpregaNet.Application.Common.Base;
using EmpregaNet.Domain.Interfaces;
using EmpregaNet.Application.Jobs.Factories;

namespace EmpregaNet.Application.Jobs.Commands
{
    public sealed record UpdateJobCommand(
        long CompanyId,
        string Title,
        string Description,
        [EnumDataType(typeof(JobTypeEnum))]
        string JobType,
        decimal Salary
    ) : IJobCommand;

    public sealed class UpdateJobHandler : IRequestHandler<UpdateCommand<UpdateJobCommand, JobViewModel>, JobViewModel>
    {
        private readonly IJobRepository _jobRepository;
        private readonly ICompanyRepository _companyRepository;
        private readonly IValidator<UpdateCommand<UpdateJobCommand, JobViewModel>> _validator;
        private readonly ILogger<UpdateJobHandler> _logger;
        public UpdateJobHandler(IJobRepository jobRepository,
                                ICompanyRepository companyRepository,
                                IValidator<UpdateCommand<UpdateJobCommand, JobViewModel>> validator,
                                ILogger<UpdateJobHandler> logger)
        {
            _jobRepository = jobRepository;
            _companyRepository = companyRepository;
            _validator = validator;
            _logger = logger;
        }

        public async Task<JobViewModel> Handle(UpdateCommand<UpdateJobCommand, JobViewModel> request, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Iniciando o processo de atualização da  vaga de emprego: {JobId}", request.Id);

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

                var job = await _jobRepository.GetByIdAsync(request.Id);

                if (job is null)
                {
                    _logger.LogError("Vaga de emprego não encontrada com ID: {JobId}", request.Id);
                    throw new ValidationAppException(
                        nameof(request.Id),
                        $"Vaga de emprego com ID '{request.Id}' não encontrada.",
                        DomainErrorEnum.RESOURCE_ID_NOT_FOUND);
                }

                if (job.IsDeleted)
                {
                    _logger.LogError("Tentativa de atualização de vaga de emprego excluída. ID: {JobId}", request.Id);
                    throw new ValidationAppException(
                        nameof(request.Id),
                        $"Não é possível atualizar uma vaga de emprego excluída. ID '{request.Id}' já está marcado como excluído.",
                        DomainErrorEnum.INVALID_ACTION_FOR_STATUS);
                }

                var updatedJob = JobFactory.Update(job, request.entity);
                await _jobRepository.UpdateAsync(updatedJob, cancellationToken);

                return updatedJob.ToViewModel();
            }
            catch (ValidationAppException ex)
            {
                _logger.LogWarning(ex, "Falha de validação ao atualizar vaga de emprego: {Message}. Request: {@Request}", ex.Message, request);
                throw;
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Erro de lógica de negócio ao atualizar vaga de emprego: {Message}. Request: {@Request}", ex.Message, request);
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro inesperado ao atualizar vaga de emprego (ID: {Id}). Request: {@Request}", request.Id, request);
                throw new Exception("Ocorreu um erro inesperado ao atualizar a vaga de emprego. Por favor, tente novamente mais tarde.");
            }
        }
    }
}