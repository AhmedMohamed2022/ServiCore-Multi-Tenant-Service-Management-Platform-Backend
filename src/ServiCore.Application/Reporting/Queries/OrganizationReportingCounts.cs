namespace ServiCore.Application.Reporting.Queries;

public sealed record OrganizationReportingCounts(
    int TotalCustomers,
    int TotalCategories,
    int TotalTeams,
    int TotalAgents);