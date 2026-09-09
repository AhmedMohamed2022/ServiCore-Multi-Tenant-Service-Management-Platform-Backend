namespace ServiCore.Application.Reporting.DTOs;

public sealed record CategoryTicketStatisticDto(
    Guid CategoryId,
    string CategoryName,
    int TicketCount);