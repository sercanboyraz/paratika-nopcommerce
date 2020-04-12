using System;
using System.Collections.Generic;
using System.Text;

namespace Nop.Plugin.Payment.Paratika.Helpers
{
    public class InstallmentList
    {
        public string count { get; set; }
        public int customerCostCommissionRate { get; set; }
        public decimal customerCostCommissionPrice { get; set; }
        
    }
}
