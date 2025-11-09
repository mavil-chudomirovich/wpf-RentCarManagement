using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HyCatTeamWPF.Models
{
    public class MyContractViewRes
    {
        public Guid Id { get; set; }
        public VehicleViewRes Vehicle { get; set; }
        public DateTimeOffset StartDate { get; set; }
        public DateTimeOffset EndDate { get; set; }
        public int Status { get; set; }
        public UserProfileViewRes Customer { get; set; }
        public StationViewRes Station { get; set; }
        public string Description { get; set; } = null!;
    }

}
