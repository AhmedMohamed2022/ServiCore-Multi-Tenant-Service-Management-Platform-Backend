# ServiCore

**ServiCore** is a multi-tenant customer support and ticket management platform built with **ASP.NET Core 8, Entity Framework Core 8, SQL Server, Angular 19, and JWT-based authentication**.

Organizations can manage teams, categories, customers, tickets, assignments, comments, notifications, and reports while maintaining strict tenant isolation and role-based access control.

### Live Demo

**Frontend:** https://servicore-lovat.vercel.app/

<img width="3812" height="1735" alt="dashboard" src="https://github.com/user-attachments/assets/5fc1357a-b534-4a60-94a2-ca59bb7a8cfe" />


> The backend is deployed separately and the Angular frontend communicates with it through the production API.

### Repositories

- **Backend:** https://github.com/AhmedMohamed2022/ServiCore-Multi-Tenant-Service-Management-Platform-Backend
- **Frontend:** https://github.com/AhmedMohamed2022/ServiCore-Multi-Tenant-Service-Management-Platform-Frontend

---

## Why I built ServiCore

A basic ticketing application is relatively straightforward: create a ticket, assign it, and close it.

The interesting engineering problems appear when the system becomes a **multi-tenant application** where:

- multiple organizations share the same application;
- users can belong to different organizations;
- the same user can have different roles in different organizations;
- customers must never access another organization's data;
- managers need scoped access to their teams;
- agents should only work on tickets assigned to them;
- business rules must remain enforced even when the API is called directly.

ServiCore was built around these problems rather than treating them as frontend concerns.

---

## Core Features

### Multi-Tenancy

- Organization-scoped data access.
- Tenant resolution through authenticated requests and organization context.
- Tenant membership validation before accessing organization resources.
- Protection against cross-organization data access.
- A single account can belong to multiple organizations.

<img width="3820" height="1735" alt="tenant-switching" src="https://github.com/user-attachments/assets/4e9d5417-c8f1-4e32-ade4-adf07a28e90d" />


### Role-Based Access Control

Staff roles:

- **Owner** — organization-wide management and reporting.
- **Manager** — manages assigned teams, tickets, categories, and scoped reports.
- **Agent** — works on tickets assigned to them.

Customers are modeled separately from organization staff roles and access the system through a dedicated customer portal.

Authorization is enforced at the **API/application level**, not only through frontend route guards or UI visibility.

### Ticket Management

ServiCore implements a domain-driven ticket lifecycle:

```text
New
 ↓
Open
 ↓
In Progress
 ↓
Waiting for Customer
 ↓
Resolved
 ↓
Closed
```

Ticket transitions are protected by domain rules and application-level authorization.

The system supports:

- Team assignment
- Agent assignment
- Ticket priorities
- Ticket categories
- Customer-created tickets
- Staff-created tickets
- Ticket comments
- Ticket status transitions
- Assignment notifications
- Resolution and closure timestamps
- Ticket history/audit timestamps

<img width="3810" height="1735" alt="tickets" src="https://github.com/user-attachments/assets/90eafc1f-a25b-42f0-b1b6-3e7ee82f1da3" />


<img width="3812" height="1745" alt="ticket-details" src="https://github.com/user-attachments/assets/f2982da7-85c3-4fcb-890c-315f57aaf376" />



### Customer Portal

Customers have a separate experience from staff.

The portal supports:

- Discovering organizations linked to the customer account.
- Selecting an organization without manually entering an internal organization ID.
- Creating tickets.
- Viewing their own tickets.
- Following ticket status.
- Participating in ticket conversations.

A customer can interact with multiple organizations without exposing the existence or data of one organization to another.

<img width="3805" height="1710" alt="customer-portal" src="https://github.com/user-attachments/assets/6b28b58d-cd92-4ba0-8395-04b8a25b2491" />


### Teams & Categories

Organizations can manage:

- Teams
- Team membership
- Categories
- Active/inactive teams
- Staff assignments

Managers have team-scoped management capabilities while Owners retain organization-wide control.

<img width="3807" height="1825" alt="team-management" src="https://github.com/user-attachments/assets/1038b8ac-ed03-4e9f-b6c4-e344696c4439" />


### Notifications & Realtime Communication

ServiCore combines persistent notifications with **SignalR** realtime communication.

Supported events include:

- Ticket created
- Ticket assigned
- Team assigned
- Comment added
- Status changed
- Ticket resolved
- Ticket closed

Realtime connections are tenant-aware and user-scoped.

### Email Invitations

The platform supports staff and customer invitation workflows using email-based tokens.

Invitation tokens are hashed before storage and are never returned directly by the API.

---

## Reporting

The backend provides organization-aware reporting endpoints for:

- Dashboard statistics
- Tickets
- Teams
- Agents
- Customers
- Categories
- Ticket time-series data

Reports are protected by authorization policies and scoped according to the user's organization role.

---

## Architecture

ServiCore follows a layered architecture:

```text
                    ┌─────────────────────┐
                    │      Angular 19     │
                    │      Frontend       │
                    └──────────┬──────────┘
                               │ HTTP / SignalR
                               ▼
                    ┌─────────────────────┐
                    │   ServiCore.API     │
                    │ Controllers / Hub   │
                    └──────────┬──────────┘
                               │
                               ▼
                    ┌─────────────────────┐
                    │ Application Layer   │
                    │ Services / DTOs /   │
                    │ Authorization       │
                    └──────────┬──────────┘
                               │
                               ▼
                    ┌─────────────────────┐
                    │   Domain Layer      │
                    │ Entities / Business │
                    │ Rules / Lifecycle   │
                    └──────────┬──────────┘
                               │
                               ▼
                    ┌─────────────────────┐
                    │ Infrastructure      │
                    │ EF Core / Identity  │
                    │ SQL Server / Email  │
                    └─────────────────────┘
```

Backend project structure:

```text
ServiCore
├── ServiCore.API
├── ServiCore.Application
├── ServiCore.Domain
└── ServiCore.Infrastructure
```

The dependency direction keeps domain logic independent from infrastructure concerns.

---

## Security & Authorization

Security was treated as a backend responsibility rather than a frontend feature.

Key mechanisms include:

- ASP.NET Core Identity
- JWT bearer authentication
- Policy-based authorization
- Organization-aware authorization requirements
- Tenant resolution middleware
- Tenant membership validation
- Team-scoped manager permissions
- Agent assignment validation
- Customer ownership checks
- SignalR tenant/user isolation
- Protected invitation tokens
- CORS configuration
- Production secrets supplied through environment variables

The frontend hides unavailable actions for usability, but the backend remains the final enforcement boundary.

---

## Technology Stack

| Area | Technology |
|---|---|
| Backend | ASP.NET Core 8 Web API |
| ORM | Entity Framework Core 8 |
| Database | SQL Server |
| Authentication | ASP.NET Core Identity + JWT |
| Realtime | SignalR |
| Email | MailKit / Brevo SMTP |
| Frontend | Angular 19 |
| UI | Tailwind CSS 3 |
| Architecture | Layered Architecture |
| Testing | xUnit / Integration Tests |
| Backend Hosting | MonsterASP |
| Frontend Hosting | Vercel |

---

## Database & Migrations

The project uses **EF Core code-first migrations**.

The repository contains the complete migration history required to create the database schema.

For local development:

```bash
cd src/ServiCore.API

dotnet ef database update \
  --project ../ServiCore.Infrastructure \
  --startup-project .
```

Production database migrations are applied deliberately rather than automatically on application startup.

---

## Deployment

The production architecture is:

```text
                    Internet
                       │
          ┌────────────┴────────────┐
          │                         │
          ▼                         ▼
     Vercel                    MonsterASP
   Angular SPA              ASP.NET Core API
          │                         │
          │                         ▼
          │                    SQL Server
          │
          └────── HTTPS / API ─────┘
```

The frontend receives the production API URL through an environment variable:

```text
SERVICORE_API_BASE_URL
```

Backend secrets and deployment-specific configuration are supplied through environment variables rather than committed configuration files.

---

## Production Debugging Experience

One of the most useful issues encountered during deployment was a production failure in the customer organization-selection flow.

The endpoint returned `500` because the EF Core model snapshot and the actual database migration history were out of sync around the `Customer.UserId` column.

The issue was traced through the database/migration state and fixed by adding the missing migration logic so that both existing and fresh databases could reach the expected schema.

This was a useful reminder that:

> A correct EF Core model does not guarantee that the deployed database schema matches it.

Migration history is part of the application's production state and must be treated accordingly.

---

## Demo Data

The development environment includes two seeded organizations:

### Brightwave IT Solutions

- 2 managers
- 6 agents
- 14 customers
- 9 customers with portal accounts
- 78 tickets
- 90 days of ticket history
- Tickets distributed across the supported lifecycle states

### Meridian Facilities Group

- Organization-specific staff and customers
- 30 tickets
- 50 days of ticket history

The seed data also demonstrates tenant-specific roles.

For example, the same account can have a different role in each organization:

| User | Brightwave | Meridian |
|---|---|---|
| Daniel Harper | Owner | Manager |
| Priya Nair | Manager | Agent |
| Tomas Rivera | Agent | Agent |

This demonstrates one of the central authorization requirements of the application: **role membership is organization-specific rather than globally attached to the user.**

---

## Running Locally

### Backend

```bash
cd src/ServiCore.API

dotnet restore

dotnet ef database update \
  --project ../ServiCore.Infrastructure \
  --startup-project .

dotnet run
```

Configure development database and SMTP settings using local configuration or .NET User Secrets.

### Frontend

```bash
cd ServiCore.Client

npm install
ng serve
```

The frontend development environment points to the local API.

---

## Testing

The backend includes unit and integration test projects covering important application behavior, including:

- Authentication
- Tenant isolation
- Role-based authorization
- Team membership
- Ticket visibility
- Ticket assignment
- Customer ownership
- Organization boundaries
- Notification behavior

The integration test suite uses a dedicated test database rather than the normal development database.

---

## Roadmap

Potential future improvements include:

### Ticketing

- Advanced ticket search, filtering, sorting, and pagination
- SLA and deadline management
- Ticket attachments
- Configurable ticket-routing rules
- Ticket archiving/soft deletion

### Customer Experience

- Knowledge base
- Customer satisfaction / CSAT
- Improved organization branding
- Customer-facing notifications and preferences

### Administration

- Pending invitation management and revocation
- Audit log viewer
- More advanced organization administration
- Subscription and usage management

### Platform

- Background job processing
- Enhanced observability and error tracking
- CI/CD pipeline
- Expanded end-to-end testing
- Performance optimization based on production metrics

These features are intentionally outside the current v1 scope.

---

## Project Status

**ServiCore v1.0 — Deployed**

The current release provides a complete multi-tenant ticket management workflow with:

- Organization isolation
- Staff and customer identities
- Organization-specific roles
- Team management
- Ticket lifecycle management
- Customer portal
- Ticket comments
- Realtime notifications
- Email invitations
- Reporting
- SQL Server persistence
- Production deployment

The application is considered a stable v1 foundation for future workflow, automation, and SaaS capabilities.
