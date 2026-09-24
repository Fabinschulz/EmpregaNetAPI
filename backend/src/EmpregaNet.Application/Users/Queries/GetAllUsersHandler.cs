using EmpregaNet.Application.Auth;
using EmpregaNet.Application.Common.Base;
using EmpregaNet.Application.Common.Exceptions;
using EmpregaNet.Application.Users.ViewModel;
using EmpregaNet.Application.Utils.CustomValidation;
using EmpregaNet.Application.Utils.Helpers;
using EmpregaNet.Domain.Common;
using EmpregaNet.Domain.Entities;
using EmpregaNet.Domain.Enums;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace EmpregaNet.Application.Users.Queries;

/// <summary>
/// Lista usuários (admin). <paramref name="IsDeleted"/>: null = todos; false = somente ativos; true = somente excluídos.
/// <paramref name="Search"/> filtra por nome de usuário ou e-mail (case-insensitive); quando o termo
/// é composto só por dígitos, procura também pelo CPF (a máscara digitada é ignorada).
/// <paramref name="UserType"/> filtra pelo nome do <see cref="UserTypeEnum"/> (case-insensitive; o rótulo
/// pt-BR não é aceito); valor desconhecido ou <c>NaoSelecionado</c> é recusado.
/// </summary>
public sealed record GetAllUsersQuery(
    int Page, int Size, string? OrderBy, bool? IsDeleted = null, string? Search = null, string? UserType = null)
    : IRequest<ListDataPagination<UserViewModel>>, IPaginatedQuery;

public sealed class GetAllUsersHandler : IRequestHandler<GetAllUsersQuery, ListDataPagination<UserViewModel>>
{
    private readonly UserManager<User> _userManager;
    private readonly IHttpCurrentUser _httpCurrentUser;

    public GetAllUsersHandler(UserManager<User> userManager, IHttpCurrentUser httpCurrentUser)
    {
        _userManager = userManager;
        _httpCurrentUser = httpCurrentUser;
    }

    public async Task<ListDataPagination<UserViewModel>> Handle(GetAllUsersQuery request, CancellationToken cancellationToken)
    {
        AdministradorAccess.EnsureAdministrator(_httpCurrentUser);

        var query = _userManager.Users.AsNoTracking();

        if (request.IsDeleted.HasValue)
            query = query.Where(u => u.IsDeleted == request.IsDeleted.Value);

        if (!string.IsNullOrWhiteSpace(request.UserType))
        {
            if (!EnumNameParser.TryParseName<UserTypeEnum>(request.UserType, out var userType) ||
                userType == UserTypeEnum.NaoSelecionado)
            {
                throw new ValidationAppException(
                    nameof(request.UserType),
                    "Tipo de usuário inválido para filtro.",
                    DomainErrorEnum.INVALID_QUERY_FILTER);
            }

            query = query.Where(u => u.UserType == userType);
        }

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var term = request.Search.Trim().ToLower();
            var digits = BrazilianDocument.NormalizeCpf(term);
            var searchesCpf = digits.Length > 0;

            query = query.Where(u =>
                (u.UserName != null && u.UserName.ToLower().Contains(term)) ||
                (u.Email != null && u.Email.ToLower().Contains(term)) ||
                (searchesCpf && u.Cpf != null && u.Cpf.Contains(digits)));
        }

        query = request.OrderBy switch
        {
            "createdAt_ASC" => query.OrderBy(x => x.CreatedAt),
            "createdAt_DESC" => query.OrderByDescending(x => x.CreatedAt),
            "updatedAt_ASC" => query.OrderBy(x => x.UpdatedAt),
            "updatedAt_DESC" => query.OrderByDescending(x => x.UpdatedAt),
            "id_ASC" => query.OrderBy(x => x.Id),
            "id_DESC" => query.OrderByDescending(x => x.Id),
            _ => query.OrderByDescending(x => x.CreatedAt)
        };

        var totalItems = await query.CountAsync(cancellationToken);
        var data = await query
            .Skip((request.Page - 1) * request.Size)
            .Take(request.Size)
            .ToListAsync(cancellationToken);

        return new ListDataPagination<UserViewModel>(
            data.Select(u => u.ToViewModel()).ToList(),
            totalItems,
            request.Page,
            request.Size);
    }
}
