using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HyCatTeamWPF.Constant
{
    public enum RentalContractStatus
    {
        RequestPeding = 0,
        PaymentPending = 1,
        Active = 2,
        Returned = 3,
        Completed = 4,
        Cancelled = 5,
        UnavailableVehicle = 6,
        RefundPending = 7
    }
}
