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
        /// <summary>
        /// Paratika API MerchantUser sample: sercan@sercan.com
        /// </summary>
        public string Username { get; set; }
        /// <summary>
        /// Paratika MerchantPassword sample: 123123123
        /// </summary>
        public string Password { get; set; }
        /// <summary>
        /// Paratika MerchantId sample: 10000023231
        /// </summary>
        public string Code { get; set; }
        /// <summary>
        /// Paratika API URL sample: https://entegrasyon.paratika.com.tr/paratika/api/v2/
        /// </summary>
        public string URL { get; set; }
        /// <summary>
        /// Paratika Hosted Page integrate with payment
        /// </summary>
        public bool PaymentHPMethod { get; set; }
        /// <summary>
        /// Paratika Hosted Page integrate URL sample:https://entegrasyon.paratika.com.tr/payment/
        /// </summary>
        public string PaymentHPMethodURL { get; set; }
        /// <summary>
        /// Paratika redirect URL sample:https://entegrasyon.paratika.com.tr/
        /// </summary>
        public string MainURL { get; set; }

    }
}
