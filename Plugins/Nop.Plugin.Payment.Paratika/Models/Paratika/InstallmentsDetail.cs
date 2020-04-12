using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nop.Plugin.Payment.Paratika.Helpers
{
    public class InstallmentsDetail
    {
        public string responseCode { get; set; }
        public string responseMsg { get; set; }
        public InstallmentPaymentSystem installmentPaymentSystem { get; set; }
    }
}
