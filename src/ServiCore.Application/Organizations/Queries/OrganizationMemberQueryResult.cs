using ServiCore.Domain.Enums;

namespace ServiCore.Application.Organizations.Queries;

public sealed record OrganizationMemberQueryResult(
    Guid UserId,
    string UserName,
    OrganizationRole Role);
