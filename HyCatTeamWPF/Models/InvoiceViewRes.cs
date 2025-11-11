using HyCatTeamWPF.Constant;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HyCatTeamWPF.Models
{
    public class InvoiceViewRes
    {
        public Guid Id { get; set; }
        public IEnumerable<InvoiceItemViewRes> InvoiceItems { get; set; } = null!;
        public DepositViewRes? Deposit { get; set; }
        public decimal Subtotal { get; set; }
        public decimal Tax { get; set; }
        public decimal Total { get; set; }
        public int Status { get; set; } = (int)InvoiceStatus.Pending;
        public int Type { get; set; }
    }
}
