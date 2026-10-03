# ServiCore API

ASP.NET Core 8 backend for [ServiCore](https://servicore-lovat.vercel.app/), a multi-tenant service management platform.
**Frontend repo:** https://github.com/AhmedMohamed2022/ServiCore-Multi-Tenant-Service-Management-Platform-Frontend

## Architecture

```
src/
  ServiCore.Domain          entities, enums (no dependencies)
  ServiCore.Application     services, DTOs, interfaces, Result type
  ServiCore.Infrastructure  EF Core, Identity, JWT, email, tenant resolution
  ServiCore.API             controllers, SignalR hubs, middleware
tests/
  ServiCore.UnitTests
  ServiCore.IntegrationTests
```

Highlights: tenant-resolution middleware, organization-role authorization handlers (Owner / Manager / Agent), customer data isolation, SignalR notification and comment publishers, transactional registration (user + organization + owner membership), email invitations via MailKit.

## Setup

Requires .NET 8 SDK and SQL Server (LocalDB or full).

```bash
# Secrets (never commit these)
cd src/ServiCore.API
dotnet user-secrets set "Jwt:SecretKey" "<random string, 32+ chars>"
dotnet user-secrets set "Email:SmtpUsername" "<smtp user>"
dotnet user-secrets set "Email:SmtpPassword" "<smtp password>"

# Database (connection string is in appsettings.Development.json)
dotnet ef database update --project ../ServiCore.Infrastructure --startup-project .

dotnet run         # Swagger is available in Development
```

The API allows CORS only from `Cors:AllowedOrigins` (default `http://localhost:4200`) and builds invitation links from `Frontend:BaseUrl`. Set both for any deployed environment.

Passwords require 8+ characters with upper, lower, digit and symbol.

## Tests

```bash
dotnet test
```

Integration tests need a reachable SQL Server instance and cover authentication, organizations, teams, tenant security, customer isolation, ticket visibility, workflow and notifications.

## Deployment

Configure `ConnectionStrings:DefaultConnection`, `Jwt:*`, `Email:*`, `Frontend:BaseUrl` and `Cors:AllowedOrigins` through environment variables or your host's secret store.
