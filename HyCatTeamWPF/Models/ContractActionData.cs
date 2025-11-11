using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace HyCatTeamWPF.Models
{
    namespace HyCatTeamWPF.Models
    {
        public record ContractActionData(Guid ContractId, bool HasVehicle)
        {
            public CheckBox CustomerSignCheckbox { get; init; }
            public CheckBox StaffSignCheckbox { get; init; }
        };
    }

}
