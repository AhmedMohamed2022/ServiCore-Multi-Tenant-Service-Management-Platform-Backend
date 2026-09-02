using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using ServiCore.Application.Common.Results;
using ServiCore.Application.Organizations.DTOs;

namespace ServiCore.Application.Organizations.Interfaces;

public interface IOrganizationService
{
    Task<Result<OrganizationDto>> CreateAsync(
        string name,
        CancellationToken cancellationToken = default);
}