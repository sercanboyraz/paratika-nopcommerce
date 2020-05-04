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
        public PaymentInfoModel()
        {

        }

        [NopResourceDisplayName("Plugins.Payments.Paratika.Fields.CardholderName")]
        public string CardholderName { get; set; }

        [NopResourceDisplayName("Plugins.Payments.Paratika.Fields.CardNumber")]
        public string CardNumber { get; set; }

        [NopResourceDisplayName("Plugins.Payments.Paratika.Fields.ExpirationDate")]
        public string ExpireCardMounth { get; set; }

        [NopResourceDisplayName("Plugins.Payments.Paratika.Fields.ExpirationYear")]
        public string ExpireCardYear { get; set; }

        [NopResourceDisplayName("Plugins.Payments.Paratika.Fields.CardCode")]
        public string CardCode { get; set; }

        public string Error { get; set; }

        public string PaymentHPMethodURL { get; set; }
        
        public string SessionToken { get; set; }

        [NopResourceDisplayName("Plugins.Payments.Paratika.Fields.Installment")]
        public int Installment { get; set; }
    }
}
