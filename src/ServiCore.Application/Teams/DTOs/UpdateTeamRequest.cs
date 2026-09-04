namespace ServiCore.Application.Teams.DTOs;

public record UpdateTeamRequest(
    string Name,
    string? Description);