using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using ServiCore.Application.Common.Interfaces;

namespace ServiCore.Infrastructure.Common;

public class TenantContext : ITenantContext
{
    public Guid? OrganizationId { get; set; }
}