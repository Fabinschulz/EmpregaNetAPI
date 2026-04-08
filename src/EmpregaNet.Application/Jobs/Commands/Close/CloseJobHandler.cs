using EmpregaNet.Application.Auth;
using EmpregaNet.Application.Common.Exceptions;
using EmpregaNet.Application.Interfaces;
using EmpregaNet.Domain.Enums;
using EmpregaNet.Domain.Interfaces;
using FluentValidation;
using Microsoft.Extensions.Logging;

namespace EmpregaNet.Application.Jobs.Commands;

public sealed record CloseJobCommand(long JobId) : IRequest<bool>, ITransactional;

public sealed class CloseJobHandler : IRequestHandler<CloseJobCommand, bool>
{
    private readonly IJobRepository _jobRepository;
    private readonly IValidator<CloseJobCommand> _validator;
    private readonly ILogger<CloseJobHandler> _logger;
    private readonly IHttpCurrentUser _httpCurrentUser;
    private readonly IJobEmployerAccess _jobEmployerAccess;

    public CloseJobHandler(
        IJobRepository jobRepository,
        IValidator<CloseJobCommand> validator,
        ILogger<CloseJobHandler> logger,
        IHttpCurrentUser httpCurrentUser,
        IJobEmployerAccess jobEmployerAccess)
    {
        _jobRepository = jobRepository;
        _validator = validator;
        _logger = logger;
        _httpCurrentUser = httpCurrentUser;
        _jobEmployerAccess = jobEmployerAccess;
    }

    public async Task<bool> Handle(CloseJobCommand request, CancellationToken cancellationToken)
    {
        RecruitmentAccess.EnsureRecruitmentStaff(_httpCurrentUser);

        _logger.LogInformation("Iniciando encerramento da vaga {JobId}", request.JobId);

        try
        {
            var job = await _jobRepository.GetByIdAsync(request.JobId, cancellationToken);
            if (job is null || job.IsDeleted)
            {
                throw new ValidationAppException(
                    nameof(request.JobId),
                    $"Vaga com ID '{request.JobId}' não encontrada.",
                    DomainErrorEnum.RESOURCE_ID_NOT_FOUND);
            }

            if (!job.IsActive)
            {
                throw new ValidationAppException(
                    nameof(request.JobId),
                    "A vaga já está encerrada.",
                    DomainErrorEnum.INVALID_ACTION_FOR_STATUS);
            }

            await _jobEmployerAccess.EnsureCanManageCompanyAsync(job.CompanyId, cancellationToken);

            job.Close();
            await _jobRepository.UpdateAsync(job, cancellationToken);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro inesperado ao tentar encerrar a vaga. Query: {@Query}", request);
            throw;
        }
    }
}
