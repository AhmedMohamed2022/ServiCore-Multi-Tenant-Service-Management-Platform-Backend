using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServiCore.Application.Common.Interfaces;

public interface ITenantContext
{
    Guid? OrganizationId { get; }
}
