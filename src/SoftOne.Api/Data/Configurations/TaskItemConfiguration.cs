using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SoftOne.Api.Data.Entities;
using SoftOne.Api.Data.Enums;

namespace SoftOne.Api.Data.Configurations;

public class TaskItemConfiguration : IEntityTypeConfiguration<TaskItem>
{
    public void Configure(EntityTypeBuilder<TaskItem> builder)
    {
        builder.ToTable("Tasks");

        builder.HasKey(t => t.Id);

        builder.Property(t => t.Title)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(t => t.Description)
            .HasMaxLength(500);

        builder.Property(t => t.IsCompleted)
            .IsRequired()
            .HasDefaultValue(false);

        builder.Property(t => t.Status)
            .IsRequired()
            .HasConversion<int>()
            .HasDefaultValue(Status.Todo);

        builder.Property(t => t.Priority)
            .IsRequired()
            .HasConversion<int>();

        builder.Property(t => t.DueDate)
            .HasColumnType("datetime2");

        builder.Property(t => t.IsDeleted)
            .IsRequired()
            .HasDefaultValue(false);

        builder.Property(t => t.CreatedAt)
            .IsRequired()
            .HasColumnType("datetime2")
            .HasDefaultValueSql("GETUTCDATE()");

        builder.Property(t => t.UpdatedAt)
            .HasColumnType("datetime2");

        builder.HasQueryFilter(t => !t.IsDeleted);

        builder.HasIndex(t => t.IsDeleted);
        builder.HasIndex(t => t.IsCompleted);
        builder.HasIndex(t => t.Status);
        builder.HasIndex(t => t.Priority);
    }
}
