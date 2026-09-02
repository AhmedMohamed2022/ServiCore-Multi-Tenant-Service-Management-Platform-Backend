using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServiCore.Domain.Enums;
public enum TicketStatus
{
    New = 1,
    Open = 2,
    InProgress = 3,
    WaitingForCustomer = 4,
    Resolved = 5,
    Closed = 6
}