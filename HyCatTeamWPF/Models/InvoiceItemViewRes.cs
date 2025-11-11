using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HyCatTeamWPF.Models
{
    public class InvoiceItemViewRes
    {
        public Guid Id { get; set; }
        public string? Description { get; set; }
        public decimal SubTotal { get; set; }
    }
}
