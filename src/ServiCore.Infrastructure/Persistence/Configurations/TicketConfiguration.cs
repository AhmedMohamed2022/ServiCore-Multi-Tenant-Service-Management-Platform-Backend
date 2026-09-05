using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ServiCore.Domain.Entities;

namespace ServiCore.Infrastructure.Persistence.Configurations;

public class TicketConfiguration : IEntityTypeConfiguration<Ticket>
{
    public void Configure(EntityTypeBuilder<Ticket> builder)
    {
        builder.ToTable("Tickets");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Title)
            .IsRequired()
            .HasMaxLength(250);

        builder.Property(x => x.Description)
            .IsRequired()
            .HasMaxLength(5000);

        builder.Property(x => x.Priority)
            .IsRequired();

        builder.Property(x => x.Status)
            .IsRequired();

        builder.Property(x => x.CreatedAt)
            .IsRequired();

        builder.Property(x => x.UpdatedAt)
            .IsRequired();

        builder.Property(x => x.ResolvedAt);

        builder.Property(x => x.ClosedAt);

        builder.HasIndex(x => new
        {
            x.OrganizationId,
            x.Status
        });

        builder.HasIndex(x => new
        {
            x.OrganizationId,
            x.Priority
        });

        builder.HasIndex(x => new
        {
            x.OrganizationId,
            x.CustomerId
        });

        builder.HasIndex(x => new
        {
            x.OrganizationId,
            x.TeamId
        });

        builder.HasIndex(x => new
        {
            x.OrganizationId,
            x.AssignedAgentId
        });

        builder.HasIndex(x => new
        {
            x.OrganizationId,
            x.CategoryId
        });

        builder.HasOne<Customer>()
            .WithMany()
            .HasForeignKey(x => x.CustomerId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<Team>()
            .WithMany()
            .HasForeignKey(x => x.TeamId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<Category>()
            .WithMany()
            .HasForeignKey(x => x.CategoryId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}