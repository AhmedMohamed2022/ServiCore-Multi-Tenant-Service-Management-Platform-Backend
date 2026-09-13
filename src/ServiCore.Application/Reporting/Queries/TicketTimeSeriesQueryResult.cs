using ServiCore.Domain.Enums;

namespace ServiCore.Application.Reporting.Queries;

public sealed record TicketTimeSeriesQueryResult(
    DateTime Date,
    int TotalTickets,
    int NewTickets,
    int OpenTickets,
    int InProgressTickets,
    int WaitingForCustomerTickets,
    int ResolvedTickets,
    int ClosedTickets);