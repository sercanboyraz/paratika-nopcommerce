using System;
using System.Collections.Generic;
using System.Text;

namespace Nop.Plugin.Payment.Paratika.Helpers
{
    public class SaleResponse
    {
        public string action { get; set; }
        public string merchant { get; set; }
        public double amount { get; set; }
        public string currency { get; set; }
        public string apiMerchantId { get; set; }
        public string paymentSystem { get; set; }
        public string paymentSystemType { get; set; }
        public string paymentSystemEftCode { get; set; }
        public string pgTranDate { get; set; }
        public string merchantPaymentId { get; set; }
        public string pgTranId { get; set; }
        public string pgTranRefId { get; set; }
        public string pgOrderId { get; set; }
        public string responseCode { get; set; }
        public string responseMsg { get; set; }
        public string tmxSessionQueryOutput { get; set; }
    }
}
