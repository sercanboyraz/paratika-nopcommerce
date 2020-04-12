using System;
using System.Collections.Generic;
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

        public string Username { get; set; }
        public bool Username_OverrideForStore { get; set; }

        [NopResourceDisplayName("Plugins.Payments.Paratika.Fields.Password")]

        public string Password { get; set; }
        public bool Password_OverrideForStore { get; set; }

        [NopResourceDisplayName("Plugins.Payments.Paratika.Fields.Guid")]

        public string Guid { get; set; }
        public bool Guid_OverrideForStore { get; set; }

        [NopResourceDisplayName("Plugins.Payments.Paratika.Fields.Code")]

        public string Code { get; set; }
        public bool Code_OverrideForStore { get; set; }

        [NopResourceDisplayName("Payment.IsActive")]

        public bool IsActive { get; set; }

        [NopResourceDisplayName("Plugins.Payments.Paratika.Fields.AdditionalFeePercentage")]
        public bool AdditionalFeePercentage { get; set; }
        public bool AdditionalFeePercentage_OverrideForStore { get; set; }

        [NopResourceDisplayName("Plugins.Payments.Paratika.Fields.AdditionalFee")]
        public decimal AdditionalFee { get; set; }
        public bool AdditionalFee_OverrideForStore { get; set; }

        [NopResourceDisplayName("Plugins.Payments.Paratika.Fields.Mail")]

        public string URL { get; set; }
        public bool URL_OverrideForStore { get; set; }
    }
}
