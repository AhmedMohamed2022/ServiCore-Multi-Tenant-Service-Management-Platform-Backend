using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServiCore.Application.Teams.DTOs;

public record CreateTeamRequest(
    string Name,
    string? Description);