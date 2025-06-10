using EmpregaNet.Application.Common.Command;
using EmpregaNet.Application.Companies.ViewModel;
using EmpregaNet.Application.ViewModel;
using EmpregaNet.Domain.Components.Mediator.Interfaces;
using EmpregaNet.Domain.Interfaces;
using Microsoft.Extensions.Logging;

namespace EmpregaNet.Application.Companies.Queries;

public sealed class GetCompanyByIdHandler : IRequestHandler<GetByIdQuery<CompanyViewModel>, CompanyViewModel>
{
    private readonly ICompanyRepository _repository;
    private readonly ILogger<GetCompanyByIdHandler> _logger;

    public GetCompanyByIdHandler(ICompanyRepository repository, ILogger<GetCompanyByIdHandler> logger)
    {
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<CompanyViewModel> Handle(GetByIdQuery<CompanyViewModel> request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Buscando empresa por ID: {Id}", request.Id);
        try
        {
            var result = await _repository.GetByIdAsync(request.Id);
            var viewModel = new CompanyViewModel
            {
                Id = result!.Id,
                CompanyName = result.CompanyName,
                TypeOfActivity = result.TypeOfActivity,
                Address = result.Address,
                RegistrationNumber = result.RegistrationNumber,
                Email = result.Email,
                Phone = result.Phone,
                Jobs = result.Jobs?.Select(job => new JobViewModel
                {
                    Id = job.Id,
                    Title = job.Title,
                    Description = job.Description,
                    Salary = job.Salary,
                    CreatedAt = job.CreatedAt,
                    UpdatedAt = job.UpdatedAt,
                    DeletedAt = job.DeletedAt,
                    IsDeleted = job.IsDeleted
                }).ToList() ?? new List<JobViewModel>(),
                CreatedAt = result.CreatedAt,
                UpdatedAt = result.UpdatedAt,
                DeletedAt = result.DeletedAt,
                IsDeleted = result.IsDeleted
            };

            _logger.LogInformation("Empresa encontrada: {Id}, Nome: {Nome}", request.Id, viewModel?.CompanyName);
            return viewModel!;
        }
        catch (KeyNotFoundException ex)
        {
            _logger.LogWarning(ex, "Funcionalidade não encontrada ao buscar por ID: {Message}. Request: {@Request}", ex.Message, request);
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro inesperado ao buscar funcionalidade por ID: {Id}. Request: {@Request}", request.Id, request);
            throw new Exception("Ocorreu um erro inesperado ao buscar a funcionalidade. Por favor, tente novamente mais tarde.");
        }
    }
}
