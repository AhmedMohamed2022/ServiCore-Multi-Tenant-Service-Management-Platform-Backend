using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServiCore.Application.Teams.DTOs;

public record TeamDto(
    Guid Id,
    Guid OrganizationId,
    string Name,
    string? Description,
    DateTime CreatedAt);
