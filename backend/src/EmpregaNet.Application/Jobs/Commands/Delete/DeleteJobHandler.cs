using EmpregaNet.Application.Auth;
using EmpregaNet.Application.Common.Exceptions;
using EmpregaNet.Application.Abstraction;
using Microsoft.Extensions.Logging;
using EmpregaNet.Application.Common.Base;
using EmpregaNet.Domain.Enums;
using EmpregaNet.Domain.Interfaces;
using EmpregaNet.Application.Jobs.ViewModel;

namespace EmpregaNet.Application.Jobs.Commands
{
    public sealed class DeleteJobHandler : IRequestHandler<DeleteCommand<JobViewModel>, bool>
    {
        private readonly IJobRepository _repository;
        private readonly IJobApplicationRepository _jobApplicationRepository;
        private readonly ILogger<DeleteJobHandler> _logger;
        private readonly IHttpCurrentUser _httpCurrentUser;
        private readonly IJobEmployerAccess _jobEmployerAccess;

        public DeleteJobHandler(
            IJobRepository repository,
            IJobApplicationRepository jobApplicationRepository,
            ILogger<DeleteJobHandler> logger,
            IHttpCurrentUser httpCurrentUser,
            IJobEmployerAccess jobEmployerAccess)
        {
            _repository = repository ?? throw new ArgumentNullException(nameof(repository));
            _jobApplicationRepository = jobApplicationRepository ?? throw new ArgumentNullException(nameof(jobApplicationRepository));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _httpCurrentUser = httpCurrentUser;
            _jobEmployerAccess = jobEmployerAccess;
        }

        public async Task<bool> Handle(DeleteCommand<JobViewModel> request, CancellationToken cancellationToken)
        {
            RecruitmentAccess.EnsureRecruitmentStaff(_httpCurrentUser);

            _logger.LogInformation("Iniciando remoção da vaga de emprego com ID: {Id}", request.Id);

            try
            {
                var job = await _repository.GetByIdAsync(request.Id, cancellationToken);
                if (job is null || job.IsDeleted)
                {
                    throw new ValidationAppException(
                        nameof(request.Id),
                        $"Vaga com ID '{request.Id}' não encontrada.",
                        DomainErrorEnum.RESOURCE_ID_NOT_FOUND);
                }

                await _jobEmployerAccess.EnsureCanManageCompanyAsync(job.CompanyId, cancellationToken);

                if (await _jobApplicationRepository.ExistsByJobIdAsync(request.Id, cancellationToken))
                {
                    throw ValidationAppException.ForBusinessRule(
                        "Não é possível excluir uma vaga com candidaturas vinculadas. Cancele ou remova as candidaturas antes de excluir a vaga.",
                        DomainErrorEnum.INVALID_ACTION_FOR_RECORD);
                }

                await _repository.DeleteAsync(request.Id, cancellationToken);
                _logger.LogInformation("Vaga de emprego removida com sucesso. ID: {Id}", request.Id);
                return true;
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning(ex, "Vaga de emprego não encontrada para remoção: {Message}. Request: {@Request}", ex.Message, request);
                throw new KeyNotFoundException("A vaga de emprego que você está tentando remover não existe ou já foi removida.");
            }
        }
    }
}