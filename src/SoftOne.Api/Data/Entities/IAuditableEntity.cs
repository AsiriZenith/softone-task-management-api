namespace SoftOne.Api.Data.Entities;

public interface IAuditableEntity
{
    DateTime CreatedAt { get; set; }

    DateTime? UpdatedAt { get; set; }
}
