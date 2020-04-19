using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Nop.Web.Framework;
using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace Nop.Plugin.Payment.Paratika.Models
{
    public class ConfigurationModel : BaseNopModel
    {
        public ConfigurationModel()
        {

        }

        public int ActiveStoreScopeConfiguration { get; set; }

        [NopResourceDisplayName("Plugins.Payments.Paratika.Fields.Username")]
        [DataType(DataType.EmailAddress)]
        public string Username { get; set; }
        public bool Username_OverrideForStore { get; set; }

        [NopResourceDisplayName("Plugins.Payments.Paratika.Fields.Password")]
        [DataType(DataType.Password)]
        public string Password { get; set; }
        public bool Password_OverrideForStore { get; set; }

        [NopResourceDisplayName("Plugins.Payments.Paratika.Fields.Code")]

        public string Code { get; set; }
        public bool Code_OverrideForStore { get; set; }

        [NopResourceDisplayName("Plugins.Payments.Paratika.Fields.Payment.IsActive")]

        public bool IsActive { get; set; }

        [NopResourceDisplayName("Plugins.Payments.Paratika.Fields.AdditionalFeePercentage")]
        public bool AdditionalFeePercentage { get; set; }
        public bool AdditionalFeePercentage_OverrideForStore { get; set; }

        [NopResourceDisplayName("Plugins.Payments.Paratika.Fields.AdditionalFee")]
        public decimal AdditionalFee { get; set; }
        public bool AdditionalFee_OverrideForStore { get; set; }

        [NopResourceDisplayName("Plugins.Payments.Paratika.Fields.URL")]
        [DataType(DataType.Url)]
        public string APIURL { get; set; }
        public bool APIURL_OverrideForStore { get; set; }

        [NopResourceDisplayName("Plugins.Payments.Paratika.Fields.PaymentHPMethod")]
        public bool PaymentHPMethod { get; set; }
        public bool PaymentHPMethod_OverrideForStore { get; set; }

        [NopResourceDisplayName("Plugins.Payments.Paratika.Fields.PaymentHPMethodURL")]
        [DataType(DataType.Url)]
        public string PaymentHPMethodURL { get; set; }
        public bool PaymentHPMethodURL_OverrideForStore { get; set; }


        [NopResourceDisplayName("Plugins.Payments.Paratika.Fields.MainURL")]
        [DataType(DataType.Url)]
        public string MainURL { get; set; }
        public bool MainURL_OverrideForStore { get; set; }
    }
}
