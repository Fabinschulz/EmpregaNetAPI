using EmpregaNet.Application.Utils.Helpers;
using EmpregaNet.Domain.Entities;
using EmpregaNet.Domain.Enums;

namespace EmpregaNet.Application.Users.ViewModel;

public static class CandidateDetailMapper
{
    public static CandidateDetailViewModel ToCandidateDetail(
        this User user,
        IReadOnlyList<string> roles,
        IReadOnlyDictionary<ApplicationStatusEnum, int> applicationsByStatus,
        DateTimeOffset now)
    {
        return new CandidateDetailViewModel
        {
            Id = user.Id,
            Username = user.UserName ?? string.Empty,
            Email = user.Email ?? string.Empty,
            PhoneNumber = user.PhoneNumber,
            UserType = user.UserType.ToDescription(),
            Roles = roles,
            ProfilePicture = user.ProfilePicture,
            City = NullIfBlank(user.Address?.City),
            State = ResolveState(user.Address),
            Age = AgeCalculator.InCompleteYears(user.BirthDate, now),
            CreatedAt = user.CreatedAt,
            UpdatedAt = user.UpdatedAt,
            IsDeleted = user.IsDeleted,
            Applications = CandidateApplicationsSummaryViewModel.From(applicationsByStatus)
        };
    }
    
    private static string? ResolveState(Address? address)
        => address is null || address.State == UF.NaoSelecionado ? null : address.State.ToString();

    private static string? NullIfBlank(string? value)
        => string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}
