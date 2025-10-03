
using EmpregaNet.Application.Utils.Helpers;

namespace EmpregaNet.Application.Common.Base;

public class BaseViewModel
{

    private DateTimeOffset _createdAt;
    private DateTimeOffset? _updatedAt;
    private DateTimeOffset? _deletedAt;

    public string CreatedAt
    {
        get => RandomHelpers.FormatToBrasiliaTime(_createdAt);
    }

    public DateTimeOffset CreatedAtUtc
    {
        set => _createdAt = value;
    }

    public string? UpdatedAt
    {
        get => RandomHelpers.FormatToBrasiliaTime(_updatedAt);
    }

    public DateTimeOffset? UpdatedAtUtc
    {
        set => _updatedAt = value;
    }

    public string? DeletedAt
    {
        get => RandomHelpers.FormatToBrasiliaTime(_deletedAt);
    }

    public DateTimeOffset? DeletedAtUtc
    {
        set => _deletedAt = value;
    }

    // public UserViewModel? CriadoPor { get; set; }
    // public UserViewModel? AtualizadoPor { get; set; }
    public bool IsDeleted { get; set; } = false;
}
