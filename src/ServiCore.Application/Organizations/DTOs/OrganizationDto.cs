using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServiCore.Application.Organizations.DTOs;

public record OrganizationDto(
    Guid Id,
    string Name,
    DateTime CreatedAt);
