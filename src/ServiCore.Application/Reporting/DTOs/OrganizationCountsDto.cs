namespace ServiCore.Application.Reporting.DTOs;

public sealed record OrganizationCountsDto(
    int TotalCustomers,
    int TotalCategories,
    int TotalTeams,
    int TotalAgents);