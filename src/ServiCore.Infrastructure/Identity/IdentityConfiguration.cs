using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ServiCore.Infrastructure.Identity;

public class IdentityConfiguration
    : IEntityTypeConfiguration<ApplicationUser>
{
    public void Configure(
        EntityTypeBuilder<ApplicationUser> builder)
    {
        builder.ToTable("AspNetUsers");

        builder.Property(x => x.UserName)
            .HasMaxLength(256);

        builder.Property(x => x.Email)
            .HasMaxLength(256);
    }
}