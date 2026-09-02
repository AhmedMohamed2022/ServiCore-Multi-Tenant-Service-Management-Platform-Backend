using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.EntityFrameworkCore;
using ServiCore.Domain.Entities;

namespace ServiCore.Infrastructure.Persistence;

public class ServiCoreDbContext : DbContext
{
    public ServiCoreDbContext(
        DbContextOptions<ServiCoreDbContext> options)
        : base(options)
    {
    }

    public DbSet<Organization> Organizations => Set<Organization>();

    public DbSet<Team> Teams => Set<Team>();

    public DbSet<TeamMember> TeamMembers => Set<TeamMember>();

    public DbSet<Customer> Customers => Set<Customer>();

    public DbSet<Category> Categories => Set<Category>();

    public DbSet<Ticket> Tickets => Set<Ticket>();

    public DbSet<TicketComment> TicketComments => Set<TicketComment>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.ApplyConfigurationsFromAssembly(
            typeof(ServiCoreDbContext).Assembly);
    }
}
