//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;

//using Microsoft.EntityFrameworkCore;
//using ServiCore.Domain.Entities;
//using ServiCore.Domain.Enums;
//using ServiCore.IntegrationTests.Infrastructure;

//namespace ServiCore.IntegrationTests;

//public class PersistenceTests
//{
//    [Fact]
//    public async Task Organization_Should_Be_Persisted_And_Retrieved()
//    {
//        await using var context = SqlServerTestDbContextFactory.Create();

//        await context.Database.EnsureCreatedAsync();

//        var organization = new Organization("Acme Support");

//        context.Organizations.Add(organization);

//        await context.SaveChangesAsync();

//        var result = await context.Organizations
//            .SingleAsync(x => x.Id == organization.Id);

//        Assert.Equal("Acme Support", result.Name);
//        Assert.Equal(organization.Id, result.Id);
//    }
//}