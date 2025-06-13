using EmpregaNet.Application.Common.Command;
using EmpregaNet.Application.Companies.ViewModel;
using EmpregaNet.Domain.Common;
using Mediator.Interfaces;
using EmpregaNet.Domain.Interfaces;
using FluentValidation;
using Microsoft.Extensions.Logging;

namespace EmpregaNet.Application.Companies.Queries;

public sealed class GetAllValidator : AbstractValidator<GetAllQuery<CompanyViewModel>>
{
    public GetAllValidator()
    {
        RuleFor(x => x.Page)
            .NotEmpty().WithMessage("Page é obrigatório")
            .GreaterThanOrEqualTo(1).WithMessage("A página precisa ser maior ou igual a 1");

        RuleFor(x => x.Size)
            .NotEmpty().WithMessage("Size é obrigatório")
            .GreaterThanOrEqualTo(100).WithMessage("Size precisa ser maior ou igual a 100");
    }
}

public sealed class GetAllCompanyHandler : IRequestHandler<GetAllQuery<CompanyViewModel>, ListDataPagination<CompanyViewModel>>
{
    private readonly ICompanyRepository _repository;
    private readonly ILogger<GetAllCompanyHandler> _logger;
    private readonly IValidator<GetAllQuery<CompanyViewModel>> _validator;

    public GetAllCompanyHandler(ICompanyRepository repository, ILogger<GetAllCompanyHandler> logger, IValidator<GetAllQuery<CompanyViewModel>> validator)
    {
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _validator = validator ?? throw new ArgumentNullException(nameof(validator));
    }

    public async Task<ListDataPagination<CompanyViewModel>> Handle(GetAllQuery<CompanyViewModel> request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Buscando todas as empresas (Página: {Page}, Tamanho: {Size}, Ordem: {OrderBy})", request.Page, request.Size, request.OrderBy ?? "Nenhum");

        try
        {
            var validationResult = await _validator.ValidateAsync(request, cancellationToken);
            if (!validationResult.IsValid)
                throw new ValidationException(validationResult.Errors);

            var result = await _repository.GetAllAsync(request.Page, request.Size, request.OrderBy);

            var totalItems = result.Data.Count;
            var companyViewModels = result.Data.Select(c => new CompanyViewModel
            {
                Id = c.Id,
                CompanyName = c.CompanyName,
                Email = c.Email,
                Phone = c.Phone,
                Address = c.Address,
                TypeOfActivity = c.TypeOfActivity,
                RegistrationNumber = c.RegistrationNumber
            }).ToList();
            _logger.LogInformation("Total de empresas encontradas: {Count}", totalItems);
            return new ListDataPagination<CompanyViewModel>(companyViewModels, totalItems, request.Page, request.Size);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro inesperado ao buscar todas as empresas. Query: {@Query}", request);
            throw;
        }
    }
}
