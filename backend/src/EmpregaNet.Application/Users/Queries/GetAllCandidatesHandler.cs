using EmpregaNet.Application.Common.Base;
using EmpregaNet.Application.Users.ViewModel;
using EmpregaNet.Domain.Common;
using EmpregaNet.Domain.Entities;
using EmpregaNet.Domain.Enums;
using FluentValidation;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace EmpregaNet.Application.Users.Queries;

/// <summary>Lista candidatos. <paramref name="Search"/> filtra por nome de usuário ou e-mail.</summary>
public sealed record GetAllCandidatesQuery(int Page, int Size, string? OrderBy, string? Search = null)
    : IRequest<ListDataPagination<UserViewModel>>, IPaginatedQuery;

public sealed class GetAllCandidatesHandler : IRequestHandler<GetAllCandidatesQuery, ListDataPagination<UserViewModel>>
{
    private readonly UserManager<User> _userManager;
    private readonly IValidator<GetAllCandidatesQuery> _validator;

    public GetAllCandidatesHandler(UserManager<User> userManager, IValidator<GetAllCandidatesQuery> validator)
    {
        _userManager = userManager;
        _validator = validator;
    }

    public async Task<ListDataPagination<UserViewModel>> Handle(GetAllCandidatesQuery request, CancellationToken cancellationToken)
    {
        var query = _userManager.Users
            .AsNoTracking()
            .Where(u => !u.IsDeleted && u.UserType == UserTypeEnum.Candidate);

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
