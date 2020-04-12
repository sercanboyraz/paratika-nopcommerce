using System;
using System.Collections.Generic;
using System.Text;

namespace Nop.Plugin.Payment.Paratika.Helpers
{
    public class InstallmentPaymentSystem
    {
        public string name { get; set; }
        public int paymentSystem { get; set; }
        public string paymentSystemEftCode { get; set; }
        //TRY
        public string currencyCode { get; set; }
        //tr-TR
        public string displayLocales { get; set; }
        public List<InstallmentList> installmentList { get; set; }
    }
}
