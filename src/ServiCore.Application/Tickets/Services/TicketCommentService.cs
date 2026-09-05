using ServiCore.Application.Common.Interfaces;
using ServiCore.Application.Tickets.DTOs;
using ServiCore.Application.Tickets.Interfaces;
using ServiCore.Domain.Entities;
using ServiCore.Domain.Enums;

namespace ServiCore.Application.Tickets.Services;

public class TicketCommentService : ITicketCommentService
{
    private readonly IApplicationDbContext _dbContext;
    private readonly ITenantContext _tenantContext;
    private readonly ICurrentUser _currentUser;

    public TicketCommentService(
        IApplicationDbContext dbContext,
        ITenantContext tenantContext,
        ICurrentUser currentUser)
    {
        _dbContext = dbContext;
        _tenantContext = tenantContext;
        _currentUser = currentUser;
    }

    public async Task<TicketCommentDto> AddAsync(
        Guid ticketId,
        AddTicketCommentRequest request,
        CancellationToken cancellationToken = default)
    {
        var organizationId = GetOrganizationId();
        var userId = GetCurrentUserId();

        var ticket = await _dbContext.GetTicketAsync(
            organizationId,
            ticketId,
            cancellationToken);

        if (ticket is null)
            throw new KeyNotFoundException("Ticket not found.");

        if (ticket.Status == TicketStatus.Closed)
            throw new InvalidOperationException(
                "Comments cannot be added to a closed ticket.");

        var isOrganizationMember =
            await _dbContext.OrganizationMemberExistsAsync(
                organizationId,
                userId,
                cancellationToken);

        var isTicketCustomer =
            await _dbContext.CustomerOwnsTicketAsync(
                organizationId,
                ticketId,
                userId,
                cancellationToken);

        if (!isOrganizationMember && !isTicketCustomer)
            throw new UnauthorizedAccessException(
                "You are not allowed to comment on this ticket.");

        var comment = new TicketComment(
            ticketId,
            userId,
            request.Content);

        _dbContext.AddTicketComment(comment);

        await _dbContext.SaveChangesAsync(cancellationToken);

        return Map(comment);
    }

    public async Task<IReadOnlyList<TicketCommentDto>> GetAllAsync(
        Guid ticketId,
        CancellationToken cancellationToken = default)
    {
        var organizationId = GetOrganizationId();
        var userId = GetCurrentUserId();

        var ticket = await _dbContext.GetTicketAsync(
            organizationId,
            ticketId,
            cancellationToken);

        if (ticket is null)
            throw new KeyNotFoundException("Ticket not found.");

        var isOrganizationMember =
            await _dbContext.OrganizationMemberExistsAsync(
                organizationId,
                userId,
                cancellationToken);

        var isTicketCustomer =
            await _dbContext.CustomerOwnsTicketAsync(
                organizationId,
                ticketId,
                userId,
                cancellationToken);

        if (!isOrganizationMember && !isTicketCustomer)
            throw new UnauthorizedAccessException(
                "You are not allowed to view comments on this ticket.");

        var comments = await _dbContext.GetTicketCommentsAsync(
            organizationId,
            ticketId,
            cancellationToken);

        return comments
            .Select(Map)
            .ToList();
    }

    private Guid GetOrganizationId()
    {
        if (!_tenantContext.OrganizationId.HasValue)
            throw new UnauthorizedAccessException(
                "Organization context is required.");

        return _tenantContext.OrganizationId.Value;
    }

    private Guid GetCurrentUserId()
    {
        if (!_currentUser.UserId.HasValue)
            throw new UnauthorizedAccessException(
                "Authenticated user is required.");

        return _currentUser.UserId.Value;
    }

    private static TicketCommentDto Map(TicketComment comment)
    {
        return new TicketCommentDto(
            comment.Id,
            comment.TicketId,
            comment.AuthorUserId,
            comment.Content,
            comment.CreatedAt);
    }
}