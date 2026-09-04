namespace ServiCore.Application.Teams.DTOs;

public record TeamMemberDto(
    Guid TeamId,
    Guid UserId,
    DateTime JoinedAt);
