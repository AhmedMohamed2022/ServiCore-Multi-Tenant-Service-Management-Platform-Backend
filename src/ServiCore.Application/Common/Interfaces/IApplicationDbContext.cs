using ServiCore.Domain.Entities;
using ServiCore.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServiCore.Application.Common.Interfaces;

public interface IApplicationDbContext
{
    void AddOrganization(Organization organization);

    void AddOrganizationMember(OrganizationMember member);

    void AddTeam(Team team);
    Task<IReadOnlyList<Team>> GetTeamsAsync(Guid organizationId, CancellationToken cancellationToken = default);

    Task<Team?> GetTeamAsync(Guid organizationId, Guid teamId, CancellationToken cancellationToken = default);

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);

    Task<IApplicationTransaction> BeginTransactionAsync(CancellationToken cancellationToken = default);
    void AddTeamMember(TeamMember member);

    Task<bool> TeamMemberExistsAsync(Guid teamId, Guid userId, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<TeamMember>> GetTeamMembersAsync(Guid organizationId, Guid teamId, CancellationToken cancellationToken = default);

    Task<TeamMember?> GetTeamMemberAsync(Guid organizationId, Guid teamId, Guid userId, CancellationToken cancellationToken = default);

    void RemoveTeamMember(TeamMember member);
    Task<bool> OrganizationMemberExistsAsync(Guid organizationId, Guid userId, CancellationToken cancellationToken = default);
    void AddCustomer(Customer customer);

    Task<IReadOnlyList<Customer>> GetCustomersAsync(Guid organizationId, CancellationToken cancellationToken = default);

    Task<Customer?> GetCustomerAsync(Guid organizationId, Guid customerId, bool activeOnly = true, CancellationToken cancellationToken = default);

    Task<bool> CustomerEmailExistsAsync(Guid organizationId, string email, Guid? excludingCustomerId = null, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Ticket>> GetCustomerTicketsAsync(Guid organizationId, Guid userId, CancellationToken cancellationToken = default);

    Task<Ticket?> GetCustomerTicketAsync(Guid organizationId, Guid ticketId, Guid userId, CancellationToken cancellationToken = default);

    Task<bool> CustomerBelongsToUserAsync(Guid organizationId, Guid customerId, Guid userId, CancellationToken cancellationToken = default);
    void AddCategory(Category category);

    Task<IReadOnlyList<Category>> GetCategoriesAsync(Guid organizationId, CancellationToken cancellationToken = default);

    Task<Category?> GetCategoryAsync(Guid organizationId, Guid categoryId, bool activeOnly = true, CancellationToken cancellationToken = default);

    Task<bool> CategoryNameExistsAsync(Guid organizationId, string name, Guid? excludingCategoryId = null, CancellationToken cancellationToken = default);
    void AddTicket(Ticket ticket);

    Task<IReadOnlyList<Ticket>> GetTicketsAsync(Guid organizationId, CancellationToken cancellationToken = default);

    Task<Ticket?> GetTicketAsync(Guid organizationId, Guid ticketId, CancellationToken cancellationToken = default);

    Task<bool> CustomerBelongsToOrganizationAsync(Guid organizationId, Guid customerId, CancellationToken cancellationToken = default);

    Task<bool> TeamBelongsToOrganizationAsync(Guid organizationId, Guid teamId, CancellationToken cancellationToken = default);

    Task<bool> CategoryBelongsToOrganizationAsync(Guid organizationId, Guid categoryId, CancellationToken cancellationToken = default);
    Task<OrganizationRole?> GetOrganizationRoleAsync(Guid organizationId, Guid userId, CancellationToken cancellationToken = default);
    void AddTicketComment(TicketComment comment);

    Task<IReadOnlyList<TicketComment>> GetTicketCommentsAsync(Guid organizationId, Guid ticketId, CancellationToken cancellationToken = default);

    Task<bool> CustomerOwnsTicketAsync(Guid organizationId, Guid ticketId, Guid userId, CancellationToken cancellationToken = default);

    void AddOrganizationInvitation(OrganizationInvitation invitation);

    Task<OrganizationInvitation?> GetInvitationByTokenHashAsync(string tokenHash, CancellationToken cancellationToken = default);

    Task<bool> PendingInvitationExistsAsync(Guid organizationId, string email, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<OrganizationInvitation>> GetOrganizationInvitationsAsync(Guid organizationId, CancellationToken cancellationToken = default);

    Task<OrganizationInvitation?> GetOrganizationInvitationAsync(Guid organizationId, Guid invitationId, CancellationToken cancellationToken = default);
    void AddCustomerInvitation(CustomerInvitation invitation);

    Task<CustomerInvitation?> GetCustomerInvitationByTokenHashAsync(string tokenHash, CancellationToken cancellationToken = default);

    Task<bool> PendingCustomerInvitationExistsAsync(Guid organizationId, Guid customerId, CancellationToken cancellationToken = default);

    Task<CustomerInvitation?> GetCustomerInvitationAsync(Guid organizationId, Guid invitationId, CancellationToken cancellationToken = default);

    Task<Customer?> GetCustomerForOrganizationAsync(Guid organizationId, Guid customerId, CancellationToken cancellationToken = default);
    void AddNotification(Notification notification);

    Task<IReadOnlyList<Notification>> GetNotificationsAsync(Guid organizationId, Guid userId, CancellationToken cancellationToken = default);

    Task<Notification?> GetNotificationAsync(Guid organizationId, Guid userId, Guid notificationId, CancellationToken cancellationToken = default);
    Task<Guid?> GetTicketCustomerUserIdAsync(Guid organizationId, Guid ticketId, CancellationToken cancellationToken = default);

}