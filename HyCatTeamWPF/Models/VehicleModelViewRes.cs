using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HyCatTeamWPF.Models
{
    public class VehicleModelViewRes
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public decimal CostPerDay { get; set; }
        public int AvailableVehicleCount { get; set; }
        public string ImageUrl { get; set; }
        public BrandViewRes Brand { get; set; }
    }

}
