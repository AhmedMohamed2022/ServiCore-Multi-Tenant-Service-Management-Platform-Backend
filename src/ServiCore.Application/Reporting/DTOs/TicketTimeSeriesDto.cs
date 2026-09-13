namespace ServiCore.Application.Reporting.DTOs;

public sealed record TicketTimeSeriesDto(
    DateTime Date,
    int TotalTickets,
    int NewTickets,
    int OpenTickets,
    int InProgressTickets,
    int WaitingForCustomerTickets,
    int ResolvedTickets,
    int ClosedTickets);