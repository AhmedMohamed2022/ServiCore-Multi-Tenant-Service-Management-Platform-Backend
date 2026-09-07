using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ServiCore.Domain.Entities;

namespace ServiCore.Infrastructure.Persistence.Configurations;

public class NotificationConfiguration
    : IEntityTypeConfiguration<Notification>
{
    public void Configure(EntityTypeBuilder<Notification> builder)
    {
        builder.ToTable("Notifications");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.UserId)
            .IsRequired();

        builder.Property(x => x.OrganizationId)
            .IsRequired();

        builder.Property(x => x.Type)
            .IsRequired();

        builder.Property(x => x.Title)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(x => x.Message)
            .IsRequired()
            .HasMaxLength(1000);

        builder.Property(x => x.RelatedEntityId);

        builder.Property(x => x.CreatedAt)
            .IsRequired();

        builder.Property(x => x.ReadAt);

        builder.HasIndex(x => new
        {
            x.UserId,
            x.OrganizationId,
            x.CreatedAt
        });

        builder.HasIndex(x => new
        {
            x.UserId,
            x.OrganizationId,
            x.ReadAt
        });
    }
}