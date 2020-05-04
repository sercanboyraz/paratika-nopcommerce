using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Nop.Core;
using Nop.Core.Domain.Directory;
using Nop.Core.Domain.Orders;
using Nop.Core.Plugins;
using Nop.Plugin.Payment.Paratika.Models;
using Nop.Plugin.Payment.Paratika.Validators;
using Nop.Services.Common;
using Nop.Services.Configuration;
using Nop.Services.Directory;
using Nop.Services.Localization;
using Nop.Services.Orders;
using Nop.Services.Payments;
using Nop.Services.Tax;
using Nop.Web.Framework.Controllers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nop.Plugin.Payment.Paratika
{
    public class ParatikaOrderPaymentProcessor : BasePlugin, IPaymentMethod
    {
        #region Fields

        private readonly CurrencySettings _currencySettings;
        private readonly ICheckoutAttributeParser _checkoutAttributeParser;
        private readonly ICurrencyService _currencyService;
        private readonly IGenericAttributeService _genericAttributeService;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ILocalizationService _localizationService;
        private readonly IPaymentService _paymentService;
        private readonly ISettingService _settingService;
        private readonly ITaxService _taxService;
        private readonly IWebHelper _webHelper;
        private readonly IShoppingCartService _shoppingCartService;
        private readonly ParatikaOrderPaymentSettings _paratikaOrderPaymentSettings;

        #endregion

        #region Ctor

        public ParatikaOrderPaymentProcessor(CurrencySettings currencySettings,
            ICheckoutAttributeParser checkoutAttributeParser,
            ICurrencyService currencyService,
            IGenericAttributeService genericAttributeService,
            IHttpContextAccessor httpContextAccessor,
            ILocalizationService localizationService,
            IShoppingCartService shoppingCartService,
            IPaymentService paymentService,
            ISettingService settingService,
            ITaxService taxService,
            IWebHelper webHelper,
            ParatikaOrderPaymentSettings paratikaOrderPaymentSettings)
        {
            this._currencySettings = currencySettings;
            this._checkoutAttributeParser = checkoutAttributeParser;
            this._currencyService = currencyService;
            this._genericAttributeService = genericAttributeService;
            this._httpContextAccessor = httpContextAccessor;
            this._localizationService = localizationService;
            this._shoppingCartService = shoppingCartService;
            this._paymentService = paymentService;
            this._settingService = settingService;
            this._taxService = taxService;
            this._webHelper = webHelper;
            this._paratikaOrderPaymentSettings = paratikaOrderPaymentSettings;
        }


        public bool CanRePostProcessPayment(Order order)
        {
            throw new NotImplementedException();
        }

        public CapturePaymentResult Capture(CapturePaymentRequest capturePaymentRequest)
        {
            return new CapturePaymentResult { Errors = new[] { "Capture method not supported" } };
        }

        public decimal GetAdditionalHandlingFee(IList<ShoppingCartItem> cart)
        {
            return _paymentService.CalculateAdditionalFee(cart,
                _paratikaOrderPaymentSettings.AdditionalFee, _paratikaOrderPaymentSettings.AdditionalFeePercentage);
        }

        public ProcessPaymentRequest GetPaymentInfo(IFormCollection form)
        {
            return new ProcessPaymentRequest
            {
                CreditCardType = form["CreditCardType"],
                CreditCardName = form["CardholderName"],
                CreditCardNumber = form["CardNumber"],
                CreditCardExpireMonth = int.Parse(form["ExpireCardMounth"]),
                CreditCardExpireYear = int.Parse(form["ExpireCardYear"]),
                CreditCardCvv2 = form["CardCode"]
            };
        }

        public bool HidePaymentMethod(IList<ShoppingCartItem> cart)
        {
            if (_paratikaOrderPaymentSettings.ShippableProductRequired && !_shoppingCartService.ShoppingCartRequiresShipping(cart))
                return true;

            return false;
        }

        /// <summary>
        /// Post process payment (used by payment gateways that require redirecting to a third-party URL)
        /// </summary>
        /// <param name="postProcessPaymentRequest">Payment info required for an order processing</param>
        public void PostProcessPayment(PostProcessPaymentRequest postProcessPaymentRequest)
        {
            //nothing
        }

        public ProcessPaymentResult ProcessPayment(ProcessPaymentRequest processPaymentRequest)
        {
            return new ProcessPaymentResult();
        }

        public ProcessPaymentResult ProcessRecurringPayment(ProcessPaymentRequest processPaymentRequest)
        {
            return new ProcessPaymentResult { Errors = new[] { "Recurring payment not supported" } };
        }

        public CancelRecurringPaymentResult CancelRecurringPayment(CancelRecurringPaymentRequest cancelPaymentRequest)
        {
            return new CancelRecurringPaymentResult { Errors = new[] { "Recurring payment not supported" } };
        }

        public RefundPaymentResult Refund(RefundPaymentRequest refundPaymentRequest)
        {
            throw new NotImplementedException();
        }

        public IList<string> ValidatePaymentForm(IFormCollection form)
        {
            var warnings = new List<string>();

            //validate
            var validator = new PaymentInfoValidator(_localizationService);
            var model = new PaymentInfoModel
            {
                CardholderName = form["CardholderName"],
                CardNumber = form["CardNumber"],
                CardCode = form["CardCode"],
                ExpireCardMounth = form["ExpireCardMounth"],
                ExpireCardYear = form["ExpireCardYear"]
            };
            var validationResult = validator.Validate(model);
            if (!validationResult.IsValid)
                warnings.AddRange(validationResult.Errors.Select(error => error.ErrorMessage));

            return warnings;
        }

        public VoidPaymentResult Void(VoidPaymentRequest voidPaymentRequest)
        {
            return new VoidPaymentResult { Errors = new[] { "Void method not supported" } };
        }

        #endregion

        /// <summary>
        /// Gets a configuration page URL
        /// </summary>
        public override string GetConfigurationPageUrl()
        {
            return $"{_webHelper.GetStoreLocation()}Admin/Paratika/Configure";
        }

        /// <summary>
        /// Gets a name of a view component for displaying plugin in public store ("payment info" checkout step)
        /// </summary>
        /// <returns>View component name</returns>
        public string GetPublicViewComponentName()
        {
            return "Paratika";
        }


        #region installAndUninstall
        public override void Install()
        {
            //settings
            _settingService.SaveSetting(new ParatikaOrderPaymentSettings
            {

            });

            //locales
            _localizationService.AddOrUpdatePluginLocaleResource("Plugins.Payments.Paratika.Fields.Error3", "Güvenli ödemeye bağlanılamadı. Tekrar edeneyiniz!", "tr-TR");
            _localizationService.AddOrUpdatePluginLocaleResource("Plugins.Payments.Paratika.Fields.Error2", "MerchantPaymentId hatası oluştu. Lütfen site yöneticisine durumu bildiriniz.", "tr-TR");
            _localizationService.AddOrUpdatePluginLocaleResource("Plugins.Payments.Paratika.Fields.Error", "Lütfen sayfayı yenileyerek tekrar deneyiniz!!", "tr-TR");
            _localizationService.AddOrUpdatePluginLocaleResource("Plugins.Payments.Paratika.Fields.Installment", "Taksit Seçiniz", "tr-TR");
            _localizationService.AddOrUpdatePluginLocaleResource("Plugins.Payments.Paratika.Fields.CardCode", "Güvenlik Kodu", "tr-TR");
            _localizationService.AddOrUpdatePluginLocaleResource("Plugins.Payments.Paratika.Fields.ExpirationYear", "Son Kul. Yıl", "tr-TR");
            _localizationService.AddOrUpdatePluginLocaleResource("Plugins.Payments.Paratika.Fields.ExpirationDate", "Son Kul. Ay", "tr-TR");
            _localizationService.AddOrUpdatePluginLocaleResource("Plugins.Payments.Paratika.Fields.CardNumber", "Kart Numarası", "tr-TR");
            _localizationService.AddOrUpdatePluginLocaleResource("Plugins.Payments.Paratika.Fields.CardholderName", "Kart Üzerindeki İsim Soyisim", "tr-TR");
            _localizationService.AddOrUpdatePluginLocaleResource("Plugins.Payments.Paratika.Fields.APIURL", "API URL", "tr-TR");
            _localizationService.AddOrUpdatePluginLocaleResource("Plugins.Payments.Paratika.Fields.AdditionalFee", "Ek Ücret Miktarı", "tr-TR");
            _localizationService.AddOrUpdatePluginLocaleResource("Plugins.Payments.Paratika.Fields.AdditionalFeePercentage", "Ek Ücret Ekle", "tr-TR");
            _localizationService.AddOrUpdatePluginLocaleResource("Plugins.Payments.Paratika.Fields.Payment.IsActive", "Aktif Mi", "tr-TR");
            _localizationService.AddOrUpdatePluginLocaleResource("Plugins.Payments.Paratika.Fields.Code", "Merchent No", "tr-TR");
            _localizationService.AddOrUpdatePluginLocaleResource("Plugins.Payments.Paratika.Fields.Password", "Merchent Şifre", "tr-TR");
            _localizationService.AddOrUpdatePluginLocaleResource("Plugins.Payments.Paratika.Fields.Username", "Merchent Adı", "tr-TR");
            _localizationService.AddOrUpdatePluginLocaleResource("Plugins.Payments.Paratika.Fields.PaymentHPMethod", "Ortak Ödeme Aktif mi", "tr-TR");
            _localizationService.AddOrUpdatePluginLocaleResource("Plugins.Payments.Paratika.Fields.PaymentHPMethodURL", "Ortak Ödeme URL", "tr-TR");

            _localizationService.AddOrUpdatePluginLocaleResource("Plugins.Payments.Paratika.Fields.Error3", "Secure payment not connected. Try again!", "en-US");
            _localizationService.AddOrUpdatePluginLocaleResource("Plugins.Payments.Paratika.Fields.Error2", "MerchantPaymentId error accurred. Please website contact.", "en-US");
            _localizationService.AddOrUpdatePluginLocaleResource("Plugins.Payments.Paratika.Fields.Error", "Please refresh page and try again!", "en-US");
            _localizationService.AddOrUpdatePluginLocaleResource("Plugins.Payments.Paratika.Fields.Installment", "Installment Chosoe", "en-US");
            _localizationService.AddOrUpdatePluginLocaleResource("Plugins.Payments.Paratika.Fields.CardCode", "Security Code", "en-US");
            _localizationService.AddOrUpdatePluginLocaleResource("Plugins.Payments.Paratika.Fields.ExpirationYear", "Expiration Year", "en-US");
            _localizationService.AddOrUpdatePluginLocaleResource("Plugins.Payments.Paratika.Fields.ExpirationDate", "Expiration Date", "en-US");
            _localizationService.AddOrUpdatePluginLocaleResource("Plugins.Payments.Paratika.Fields.CardNumber", "Card Number", "en-US");
            _localizationService.AddOrUpdatePluginLocaleResource("Plugins.Payments.Paratika.Fields.CardholderName", "Cardholder Name", "en-US");
            _localizationService.AddOrUpdatePluginLocaleResource("Plugins.Payments.Paratika.Fields.APIURL", "API URL", "en-US");
            _localizationService.AddOrUpdatePluginLocaleResource("Plugins.Payments.Paratika.Fields.AdditionalFee", "Ekstra Pay", "en-US");
            _localizationService.AddOrUpdatePluginLocaleResource("Plugins.Payments.Paratika.Fields.AdditionalFeePercentage", "Ekstra Pay is Active", "en-US");
            _localizationService.AddOrUpdatePluginLocaleResource("Plugins.Payments.Paratika.Fields.Payment.IsActive", "Active", "en-US");
            _localizationService.AddOrUpdatePluginLocaleResource("Plugins.Payments.Paratika.Fields.Code", "Merchent No", "en-US");
            _localizationService.AddOrUpdatePluginLocaleResource("Plugins.Payments.Paratika.Fields.Password", "Merchent Password", "en-US");
            _localizationService.AddOrUpdatePluginLocaleResource("Plugins.Payments.Paratika.Fields.Username", "Merchent Username", "en-US");
            _localizationService.AddOrUpdatePluginLocaleResource("Plugins.Payments.Paratika.Fields.PaymentHPMethod", "HP Method Active", "en-US");
            _localizationService.AddOrUpdatePluginLocaleResource("Plugins.Payments.Paratika.Fields.PaymentHPMethodURL", "HP Method URL", "en-US");
            

            base.Install();
        }

        /// <summary>
        /// Uninstall the plugin
        /// </summary>
        public override void Uninstall()
        {
            //settings
            _settingService.DeleteSetting<ParatikaOrderPaymentSettings>();

            //locales
            _localizationService.DeletePluginLocaleResource("Plugins.Payments.Paratika.Fields.Error3");
            _localizationService.DeletePluginLocaleResource("Plugins.Payments.Paratika.Fields.Error2");
            _localizationService.DeletePluginLocaleResource("Plugins.Payments.Paratika.Fields.Error");
            _localizationService.DeletePluginLocaleResource("Plugins.Payments.Paratika.Fields.Installment");
            _localizationService.DeletePluginLocaleResource("Plugins.Payments.Paratika.Fields.CardCode");
            _localizationService.DeletePluginLocaleResource("Plugins.Payments.Paratika.Fields.ExpirationYear");
            _localizationService.DeletePluginLocaleResource("Plugins.Payments.Paratika.Fields.ExpirationDate");
            _localizationService.DeletePluginLocaleResource("Plugins.Payments.Paratika.Fields.CardNumber");
            _localizationService.DeletePluginLocaleResource("Plugins.Payments.Paratika.Fields.CardholderName");
            _localizationService.DeletePluginLocaleResource("Plugins.Payments.Paratika.Fields.APIURL");
            _localizationService.DeletePluginLocaleResource("Plugins.Payments.Paratika.Fields.AdditionalFee");
            _localizationService.DeletePluginLocaleResource("Plugins.Payments.Paratika.Fields.AdditionalFeePercentage");
            _localizationService.DeletePluginLocaleResource("Plugins.Payments.Paratika.Fields.Payment.IsActive");
            _localizationService.DeletePluginLocaleResource("Plugins.Payments.Paratika.Fields.Code");
            _localizationService.DeletePluginLocaleResource("Plugins.Payments.Paratika.Fields.Password");
            _localizationService.DeletePluginLocaleResource("Plugins.Payments.Paratika.Fields.Username");
            _localizationService.DeletePluginLocaleResource("Plugins.Payments.Paratika.Fields.PaymentHPMethod");
            _localizationService.DeletePluginLocaleResource("Plugins.Payments.Paratika.Fields.PaymentHPMethodURL");
            _localizationService.DeletePluginLocaleResource("Plugins.Payments.Paratika.Fields.MainURL");
            

            base.Uninstall();
        }
        #endregion

        #region Properties

        /// <summary>
        /// Gets a value indicating whether capture is supported
        /// </summary>
        public bool SupportCapture
        {
            get { return false; }
        }

        /// <summary>
        /// Gets a value indicating whether partial refund is supported
        /// </summary>
        public bool SupportPartiallyRefund
        {
            get { return false; }
        }

        /// <summary>
        /// Gets a value indicating whether refund is supported
        /// </summary>
        public bool SupportRefund
        {
            get { return false; }
        }

        /// <summary>
        /// Gets a value indicating whether void is supported
        /// </summary>
        public bool SupportVoid
        {
            get { return false; }
        }

        /// <summary>
        /// Gets a recurring payment type of payment method
        /// </summary>
        public RecurringPaymentType RecurringPaymentType
        {
            get { return RecurringPaymentType.Manual; }
        }

        /// <summary>
        /// Gets a payment method type
        /// </summary>
        public PaymentMethodType PaymentMethodType
        {
            get { return PaymentMethodType.Standard; }
        }

        /// <summary>
        /// Gets a value indicating whether we should display a payment information page for this plugin
        /// </summary>
        public bool SkipPaymentInfo
        {
            get { return false; }
        }

        /// <summary>
        /// Gets a payment method description that will be displayed on checkout pages in the public store
        /// </summary>
        public string PaymentMethodDescription
        {
            //return description of this payment method to be display on "payment method" checkout step. good practice is to make it localizable
            //for example, for a redirection payment method, description may be like this: "You will be redirected to PayPal site to complete the payment"
            get { return _localizationService.GetResource("Plugins.Payments.Manual.PaymentMethodDescription"); }
        }
        #endregion
    }
}
