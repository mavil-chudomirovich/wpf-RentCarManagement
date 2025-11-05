using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HyCatTeamWPF.Models
{
    public class VehicleViewRes
    {
        public Guid Id { get; set; }
        public VehicleModelViewRes Model { get; set; }
        public string LicensePlate { get; set; }
    }
}
