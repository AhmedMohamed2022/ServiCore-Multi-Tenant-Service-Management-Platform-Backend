using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.EntityFrameworkCore;
using ServiCore.Infrastructure.Persistence;

namespace ServiCore.IntegrationTests.Infrastructure;

public static class SqlServerTestDbContextFactory
{
    private const string ConnectionString =
        "Server=.;Database=ServiCoreDb_IntegrationTests;" +
        "Trusted_Connection=True;" +
        "TrustServerCertificate=True;";

    public static ServiCoreDbContext Create()
    {
        var options = new DbContextOptionsBuilder<ServiCoreDbContext>()
            .UseSqlServer(ConnectionString)
            .Options;

        return new ServiCoreDbContext(options);
    }
}
