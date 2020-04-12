using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Plugin.Payment.Paratika.Helpers;
using Nop.Web.Framework;
using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace Nop.Plugin.Payment.Paratika.Models
{
    public class PaymentInfoModel : BaseNopModel
    {
        [NopResourceDisplayName("Payment.CardholderName")]
        public string CardholderName { get; set; }

        [NopResourceDisplayName("Payment.CardNumber")]
        public string CardNumber { get; set; }

        [NopResourceDisplayName("Payment.ExpirationDate")]
        public string ExpireCardMounth { get; set; }

        [NopResourceDisplayName("Payment.ExpirationYear")]
        public string ExpireCardYear { get; set; }

        [NopResourceDisplayName("Payment.CardCode")]
        public string CardCode { get; set; }

        [NopResourceDisplayName("Plugins.Payments.Paratika.Fields.IsProcess3D")]
        public bool IsProcess3D { get; set; }
        public string Error { get; set; }

        [NopResourceDisplayName("Plugins.Payments.Paratika.Fields.Installments")]
        public string SessionToken { get; set; }

        [NopResourceDisplayName("Plugins.Payments.Paratika.Fields.Installment")]
        public int Installment { get; set; }
    }
}
