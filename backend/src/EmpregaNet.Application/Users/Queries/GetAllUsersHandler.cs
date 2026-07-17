using EmpregaNet.Application.Auth;
using EmpregaNet.Application.Common.Base;
using EmpregaNet.Application.Users.ViewModel;
using EmpregaNet.Domain.Common;
using EmpregaNet.Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace EmpregaNet.Application.Users.Queries;

/// <summary>
/// Lista usuários (admin). <paramref name="IsDeleted"/>: null = todos; false = somente ativos; true = somente excluídos.
/// <paramref name="Search"/> filtra por nome de usuário ou e-mail (case-insensitive).
/// </summary>
public sealed record GetAllUsersQuery(int Page, int Size, string? OrderBy, bool? IsDeleted = null, string? Search = null)
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

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var term = request.Search.Trim().ToLower();
            query = query.Where(u =>
                (u.UserName != null && u.UserName.ToLower().Contains(term)) ||
                (u.Email != null && u.Email.ToLower().Contains(term)));
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
