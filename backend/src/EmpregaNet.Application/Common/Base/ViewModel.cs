
using EmpregaNet.Application.Utils.Helpers;

namespace EmpregaNet.Application.Common.Base;

public class BaseViewModel
{

    private DateTimeOffset _createdAt;
    private DateTimeOffset? _updatedAt;
    private DateTimeOffset? _deletedAt;

    public string CreatedAt => RandomHelpers.FormatToBrasiliaTime(_createdAt);
    public string? UpdatedAt => RandomHelpers.FormatToBrasiliaTime(_updatedAt);
    public string? DeletedAt => RandomHelpers.FormatToBrasiliaTime(_deletedAt);


    public DateTimeOffset CreatedAtUtc { set => _createdAt = value; }
    public DateTimeOffset? UpdatedAtUtc { set => _updatedAt = value; }
    public DateTimeOffset? DeletedAtUtc { set => _deletedAt = value; }

    // public UserViewModel? CriadoPor { get; set; }
    // public UserViewModel? AtualizadoPor { get; set; }
    public bool IsDeleted { get; set; } = false;
}
