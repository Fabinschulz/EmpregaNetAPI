using System.ComponentModel.DataAnnotations;

namespace EmpregaNet.Domain.Common;

public abstract class BaseEntity : IEquatable<BaseEntity>
{
    [Key]
    public long Id { get; private set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; } = null;
    public DateTime? DeletedAt { get; set; } = null;
    public bool IsDeleted { get; set; } = false;

    public BaseEntity()
    {

    }

    public bool Equals(BaseEntity? other)
    {
        return Id == other?.Id;
    }
}
