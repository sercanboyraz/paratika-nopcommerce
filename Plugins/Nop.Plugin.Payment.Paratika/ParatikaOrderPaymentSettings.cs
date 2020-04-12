using Nop.Core.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nop.Plugin.Payment.Paratika
{
    public class ParatikaOrderPaymentSettings : ISettings
    {
        /// <summary>
        /// Gets or sets a value indicating whether to "additional fee" is specified as percentage. true - percentage, false - fixed value.
        /// </summary>
        public bool AdditionalFeePercentage { get; set; }
        /// <summary>
        /// Additional fee
        /// </summary>
        public decimal AdditionalFee { get; set; }
        /// <summary>
        /// Gets or sets a value indicating whether shippable products are required in order to display this payment method during checkout
        /// </summary>
        public bool ShippableProductRequired { get; set; }

        public string Username { get; set; }// public string ApiAccountName { get; set; }

        public string Password { get; set; }//public string ApiAccountPassword { get; set; }

        public string Code { get; set; }//public bool ApiBaseUrl { get; set; }

        public string URL { get; set; }//public bool ApiBaseUrl { get; set; }

        public string Guid { get; set; }//public bool ApiBaseUrl { get; set; }

    }
}
