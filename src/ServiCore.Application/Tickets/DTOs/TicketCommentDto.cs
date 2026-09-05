namespace ServiCore.Application.Tickets.DTOs;

public sealed record TicketCommentDto(
    Guid Id,
    Guid TicketId,
    Guid AuthorUserId,
    string Content,
    DateTime CreatedAt);