using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ServiCore.Domain.Entities;

namespace ServiCore.Infrastructure.Persistence.Configurations;

public class CustomerInvitationConfiguration
    : IEntityTypeConfiguration<CustomerInvitation>
{
    public void Configure(
        EntityTypeBuilder<CustomerInvitation> builder)
    {
        builder.ToTable("CustomerInvitations");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Email)
            .IsRequired()
            .HasMaxLength(320);

        builder.Property(x => x.TokenHash)
            .IsRequired()
            .HasMaxLength(128);

        builder.Property(x => x.CreatedAt)
            .IsRequired();

        builder.Property(x => x.ExpiresAt)
            .IsRequired();

        builder.Property(x => x.AcceptedAt);

        builder.Property(x => x.RevokedAt);

        builder.HasIndex(x => x.TokenHash)
            .IsUnique();

        builder.HasIndex(x => new
        {
            x.OrganizationId,
            x.CustomerId
        });

        builder.HasIndex(x => new
        {
            x.OrganizationId,
            x.Email,
            x.AcceptedAt,
            x.RevokedAt
        });
    }
}