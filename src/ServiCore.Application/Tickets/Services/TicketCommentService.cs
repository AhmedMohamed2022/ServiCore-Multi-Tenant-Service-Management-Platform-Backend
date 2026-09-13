using ServiCore.Application.Common.Interfaces;
using ServiCore.Application.Notifications.DTOs;
using ServiCore.Application.Notifications.Interfaces;
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
    private readonly INotificationService _notificationService;
    private readonly ITicketCommentRealtimePublisher _realtimePublisher;

    public TicketCommentService(
        IApplicationDbContext dbContext,
        ITenantContext tenantContext,
        ICurrentUser currentUser,
        INotificationService notificationService,
        ITicketCommentRealtimePublisher realtimePublisher)
    {
        _dbContext = dbContext;
        _tenantContext = tenantContext;
        _currentUser = currentUser;
        _notificationService = notificationService;
        _realtimePublisher = realtimePublisher;
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
            throw new KeyNotFoundException(
                "Ticket not found.");

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

        await _dbContext.SaveChangesAsync(
            cancellationToken);

        var commentDto = Map(comment);

        if (isTicketCustomer &&
            ticket.AssignedAgentId.HasValue)
        {
            var agentId = ticket.AssignedAgentId.Value;

            await _notificationService.CreateAsync(
                new CreateNotificationRequest(
                    UserId: agentId,
                    Type: NotificationType.TicketCommentAdded,
                    Title: "New customer comment",
                    Message:
                        $"The customer added a new comment to ticket \"{ticket.Title}\".",
                    RelatedEntityId: ticket.Id),
                cancellationToken);

            await _realtimePublisher.PublishAsync(
                agentId,
                organizationId,
                commentDto,
                cancellationToken);
        }

        if (isOrganizationMember)
        {
            var customerUserId =
                await _dbContext.GetTicketCustomerUserIdAsync(
                    organizationId,
                    ticket.Id,
                    cancellationToken);

            if (customerUserId.HasValue &&
                customerUserId.Value != userId)
            {
                await _notificationService.CreateAsync(
                    new CreateNotificationRequest(
                        UserId: customerUserId.Value,
                        Type: NotificationType.TicketCommentAdded,
                        Title: "New ticket comment",
                        Message:
                            $"A support agent added a new comment to ticket \"{ticket.Title}\".",
                        RelatedEntityId: ticket.Id),
                    cancellationToken);

                await _realtimePublisher.PublishAsync(
                    customerUserId.Value,
                    organizationId,
                    commentDto,
                    cancellationToken);
            }
        }

        return commentDto;
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
            throw new KeyNotFoundException(
                "Ticket not found.");

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

        var comments =
            await _dbContext.GetTicketCommentsAsync(
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

    private static TicketCommentDto Map(
        TicketComment comment)
    {
        return new TicketCommentDto(
            comment.Id,
            comment.TicketId,
            comment.AuthorUserId,
            comment.Content,
            comment.CreatedAt);
    }
}