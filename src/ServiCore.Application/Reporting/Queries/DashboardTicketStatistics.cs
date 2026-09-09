namespace ServiCore.Application.Reporting.Queries;

public sealed record DashboardTicketStatistics(
    int TotalTickets,
    int NewTickets,
    int OpenTickets,
    int InProgressTickets,
    int WaitingForCustomerTickets,
    int ResolvedTickets,
    int ClosedTickets,
    int LowPriorityTickets,
    int MediumPriorityTickets,
    int HighPriorityTickets,
    int CriticalPriorityTickets,
    int ResolvedCount,
    int ClosedCount,
    double? AverageResolutionSeconds,
    double? AverageClosureSeconds);