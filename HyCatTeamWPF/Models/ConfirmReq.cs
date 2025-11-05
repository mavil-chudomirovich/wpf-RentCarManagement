using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HyCatTeamWPF.Models
{
    public class ConfirmReq
    {
        public bool hasVehicle { get; set; }
        public int? vehicleStatus { get; set; } = null;
    }
}
