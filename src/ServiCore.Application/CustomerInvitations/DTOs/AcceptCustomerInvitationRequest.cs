namespace ServiCore.Application.CustomerInvitations.DTOs;

public sealed record AcceptCustomerInvitationRequest(
    string Token,
    string? Password);