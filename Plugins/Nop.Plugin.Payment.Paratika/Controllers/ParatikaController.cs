using System;
using System.IO;
using System.Net;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using Nop.Core;
using Nop.Core.Domain.Common;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Directory;
using Nop.Core.Domain.Discounts;
using Nop.Core.Domain.Orders;
using Nop.Plugin.Payment.Paratika.Helpers;
using Nop.Plugin.Payment.Paratika.Models;
using Nop.Services.Catalog;
using Nop.Services.Common;
using Nop.Services.Configuration;
using Nop.Services.Customers;
using Nop.Services.Directory;
using Nop.Services.Localization;
using Nop.Services.Logging;
using Nop.Services.Orders;
using Nop.Services.Payments;
using Nop.Services.Stores;
using Nop.Web.Framework.Controllers;
using Nop.Plugin.Payment.Paratika;
using Nop.Web.Framework.Mvc.Filters;
using Microsoft.AspNetCore.Mvc;
using Nop.Web.Framework;
using Nop.Services.Security;
using Microsoft.AspNetCore.Http;
using Nop.Core.Http.Extensions;
using Newtonsoft.Json.Linq;
using System.Reflection;
using Microsoft.AspNetCore.Cors;
using Nop.Core.Http.Extensions;

namespace Nop.Plugin.Payment.Paratika.Controllers
{
    public class ParatikaController : BasePaymentController
    {
        private readonly IWorkContext _workContext;
        private readonly IStoreService _storeService;
        private readonly IStoreContext _storeContext;
        private readonly ISettingService _settingService;
        private readonly ILocalizationService _localizationService;
        private readonly ILanguageService _languageService;
        private readonly IOrderProcessingService _orderProcessingService;
        private readonly IGenericAttributeService _genericAttributeService;
        private readonly ICurrencyService _currencyService;
        private readonly ICustomerService _customerService;
        private readonly CurrencySettings _currencySettings;
        private readonly ParatikaOrderPaymentSettings _paratikaOrderPaymentSettings;
        private readonly IOrderTotalCalculationService _orderTotalCalculationService;
        private readonly ILogger _logger;
        private readonly IPaymentService _paymentService;
        private readonly OrderSettings _orderSettings;
        private readonly IPriceFormatter _priceFormatter;
        private readonly IPermissionService _permissionService;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IOrderService _orderService;
        private readonly IWebHelper _webHelper;


        private ISession _session => _httpContextAccessor.HttpContext.Session;

        public ParatikaController(IWorkContext workContext,
            IHttpContextAccessor httpContextAccessor,
            IStoreService storeService,
            ISettingService settingService,
            IStoreContext storeContext,
            ILocalizationService localizationService,
            ILanguageService languageService,
            IOrderProcessingService orderProcessingService,
            IGenericAttributeService genericAttributeService,
            ParatikaOrderPaymentSettings ParatikaOrderPaymentSettings,
            ICurrencyService currencyService,
            IPermissionService permissionService,
            ICustomerService customerService,
            CurrencySettings currencySettings,
            IOrderService orderService,
            IWebHelper webHelper,
            IOrderTotalCalculationService orderTotalCalculationService,
            ILogger logger,
            IPaymentService paymentService,
            OrderSettings orderSettings,
            IPriceFormatter priceFormatter)
        {
            this._httpContextAccessor = httpContextAccessor;
            this._orderService = orderService;
            this._workContext = workContext;
            this._storeService = storeService;
            this._settingService = settingService;
            this._storeContext = storeContext;
            this._localizationService = localizationService;
            this._languageService = languageService;
            this._permissionService = permissionService;
            this._webHelper = webHelper;
            this._orderProcessingService = orderProcessingService;
            this._genericAttributeService = genericAttributeService;
            this._paratikaOrderPaymentSettings = ParatikaOrderPaymentSettings;
            this._currencyService = currencyService;
            this._customerService = customerService;
            this._currencySettings = currencySettings;
            this._orderTotalCalculationService = orderTotalCalculationService;
            this._logger = logger;
            this._paymentService = paymentService;
            this._orderSettings = orderSettings;
            this._priceFormatter = priceFormatter;
        }

        [AuthorizeAdmin]
        [Area(AreaNames.Admin)]
        public IActionResult Configure()
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManagePaymentMethods))
                return AccessDeniedView();

            var storeScope = _storeContext.ActiveStoreScopeConfiguration;
            var orderPaymentSettings = _settingService.LoadSetting<ParatikaOrderPaymentSettings>(storeScope);

            var model = new ConfigurationModel();
            model.Username = orderPaymentSettings.Username;
            model.Code = orderPaymentSettings.Code;
            model.Password = orderPaymentSettings.Password;
            model.AdditionalFee = orderPaymentSettings.AdditionalFee;
            model.AdditionalFeePercentage = orderPaymentSettings.AdditionalFeePercentage;
            model.APIURL = orderPaymentSettings.URL;
            model.PaymentHPMethod = orderPaymentSettings.PaymentHPMethod;
            model.PaymentHPMethodURL = orderPaymentSettings.PaymentHPMethodURL;

            model.ActiveStoreScopeConfiguration = storeScope;
            if (storeScope > 0)
            {
                model.Password_OverrideForStore = _settingService.SettingExists(orderPaymentSettings, x => x.Password, storeScope);
                model.Code_OverrideForStore = _settingService.SettingExists(orderPaymentSettings, x => x.Code, storeScope);
                model.Username_OverrideForStore = _settingService.SettingExists(orderPaymentSettings, x => x.Username, storeScope);
                model.APIURL_OverrideForStore = _settingService.SettingExists(orderPaymentSettings, x => x.URL, storeScope);
                model.PaymentHPMethod_OverrideForStore = _settingService.SettingExists(orderPaymentSettings, x => x.PaymentHPMethod, storeScope);
                model.PaymentHPMethodURL_OverrideForStore = _settingService.SettingExists(orderPaymentSettings, x => x.PaymentHPMethodURL, storeScope);
                model.AdditionalFee_OverrideForStore = _settingService.SettingExists(orderPaymentSettings, x => x.AdditionalFee, storeScope);
                model.AdditionalFeePercentage_OverrideForStore = _settingService.SettingExists(orderPaymentSettings, x => x.AdditionalFeePercentage, storeScope);
            }
            return View("~/Plugins/Payments.Paratika/Views/Configure.cshtml", model);
        }

        [AuthorizeAdmin]
        [Area(AreaNames.Admin)]
        [HttpPost]
        [AdminAntiForgery]
        public IActionResult Configure(ConfigurationModel model)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManagePaymentMethods))
                return AccessDeniedView();

            if (!ModelState.IsValid)
                return Configure();

            //load settings for a cho"" store scope
            var storeScope = _storeContext.ActiveStoreScopeConfiguration;
            var paratikaSettings = _settingService.LoadSetting<ParatikaOrderPaymentSettings>(storeScope);
            paratikaSettings.Password = model.Password;
            paratikaSettings.Username = model.Username;
            paratikaSettings.Code = model.Code;
            paratikaSettings.AdditionalFee = model.AdditionalFee;
            paratikaSettings.AdditionalFeePercentage = model.AdditionalFeePercentage;
            paratikaSettings.URL = model.APIURL;
            paratikaSettings.PaymentHPMethod = model.PaymentHPMethod;
            paratikaSettings.PaymentHPMethodURL = model.PaymentHPMethodURL;

            _settingService.SaveSettingOverridablePerStore(paratikaSettings, x => x.Password, model.Password_OverrideForStore, storeScope, false);
            _settingService.SaveSettingOverridablePerStore(paratikaSettings, x => x.Code, model.Code_OverrideForStore, storeScope, false);
            _settingService.SaveSettingOverridablePerStore(paratikaSettings, x => x.PaymentHPMethod, model.PaymentHPMethod_OverrideForStore, storeScope, false);
            _settingService.SaveSettingOverridablePerStore(paratikaSettings, x => x.PaymentHPMethodURL, model.PaymentHPMethodURL_OverrideForStore, storeScope, false);
            _settingService.SaveSettingOverridablePerStore(paratikaSettings, x => x.Username, model.Username_OverrideForStore, storeScope, false);
            _settingService.SaveSettingOverridablePerStore(paratikaSettings, x => x.URL, model.APIURL_OverrideForStore, storeScope, false);
            _settingService.SaveSettingOverridablePerStore(paratikaSettings, x => x.AdditionalFee, model.AdditionalFee_OverrideForStore, storeScope, false);
            _settingService.SaveSettingOverridablePerStore(paratikaSettings, x => x.AdditionalFeePercentage, model.AdditionalFeePercentage_OverrideForStore, storeScope, false);
            _settingService.ClearCache();

            SuccessNotification(_localizationService.GetResource("Admin.Plugins.Saved"));

            return Configure();
        }

        [HttpGet]
        public IActionResult ParatikaPaymentInfo(PaymentInfoModel model)
        {
            Dictionary<String, String> requestParameters = new Dictionary<String, String>();
            var processPaymentRequest = new ProcessPaymentRequest();
            var processGuid = Guid.NewGuid().ToString();

            if (_orderSettings.CheckoutDisabled)
                return RedirectToRoute("ShoppingCart");

            var cart = _workContext.CurrentCustomer.ShoppingCartItems
                .Where(sci => sci.ShoppingCartType == ShoppingCartType.ShoppingCart)
                .LimitPerStore(_storeContext.CurrentStore.Id)
                .ToList();
            if (!cart.Any())
                return RedirectToRoute("ShoppingCart");

            if (_orderSettings.OnePageCheckoutEnabled)
                return RedirectToRoute("CheckoutOnePage");

            if (_workContext.CurrentCustomer.IsGuest() && !_orderSettings.AnonymousCheckoutAllowed)
                return Challenge();

            //Check whether payment workflow is required
            var isPaymentWorkflowRequired = _orderProcessingService.IsPaymentWorkflowRequired(cart);
            if (!isPaymentWorkflowRequired)
            {
                return RedirectToRoute("CheckoutConfirm");
            }

            //load payment method
            var paymentMethodSystemName = _genericAttributeService.GetAttribute<string>(_workContext.CurrentCustomer,
            NopCustomerDefaults.SelectedPaymentMethodAttribute, _storeContext.CurrentStore.Id);
            var paymentMethod = _paymentService.LoadPaymentMethodBySystemName(paymentMethodSystemName);
            if (paymentMethod == null)
                return RedirectToRoute("CheckoutPaymentMethod");

            string sessionTokenValue = _session.Get<string>("SESSIONTOKEN_" + _workContext.CurrentCustomer.Id);

            //if prataka is not active, you get payment with HP Interface.
            if (_paratikaOrderPaymentSettings.PaymentHPMethod)
            {
                //used main url set value only with hp method 
                var orderGuid = _session.Get<string>("MERCHANTPAYMENTID_" + _workContext.CurrentCustomer.Id);
                processPaymentRequest.OrderGuid = Guid.Parse(orderGuid);
                HttpContext.Session.Set<ProcessPaymentRequest>("OrderPaymentInfo", processPaymentRequest);
                model.PaymentHPMethodURL = _paratikaOrderPaymentSettings.PaymentHPMethodURL + sessionTokenValue;
                return Json(model);
            }

            if (!string.IsNullOrWhiteSpace(sessionTokenValue))
            {
                model.SessionToken = sessionTokenValue;
                return View("~/Plugins/Payments.Paratika/Views/PaymentInfo.cshtml", model);
            }
            else
            {
                _logger.Information(" > SESSIONTOKEN operation is FAILED, please check the error codes.");
                model.Error = _localizationService.GetResource("Plugins.Payments.Paratika.Fields.Error");
                return View("~/Plugins/Payments.Paratika/Views/PaymentInfo.cshtml", model);
            }
        }

        [HttpGet]
        [DisableCors]
        public IActionResult ParatikaPaymentInfo3D(PaymentInfoModel model)
        {
            Dictionary<String, String> requestParameters = new Dictionary<String, String>();

            requestParameters.Add("NAMEONCARD", model.CardholderName);
            requestParameters.Add("CARDPAN", model.CardNumber);
            requestParameters.Add("CARDEXPIRY", model.ExpireCardMounth + "." + model.ExpireCardYear.ToString());
            requestParameters.Add("CARDCVV", model.CardCode);
            requestParameters.Add("ACTION", "SALE");
            requestParameters.Add("INSTALLMENTS", model.Installment.ToString());
            requestParameters.Add("cardOwner", model.CardholderName);
            requestParameters.Add("pan", model.CardNumber);
            requestParameters.Add("expiryMonth", model.ExpireCardMounth);
            requestParameters.Add("expiryYear", model.ExpireCardYear);
            requestParameters.Add("cvv", model.CardCode);
            requestParameters.Add("installmentCount", model.Installment.ToString());

            if (_orderSettings.CheckoutDisabled)
                return RedirectToRoute("ShoppingCart");

            var cart = _workContext.CurrentCustomer.ShoppingCartItems
                .Where(sci => sci.ShoppingCartType == ShoppingCartType.ShoppingCart)
                .LimitPerStore(_storeContext.CurrentStore.Id)
                .ToList();
            if (!cart.Any())
                return RedirectToRoute("ShoppingCart");

            if (_orderSettings.OnePageCheckoutEnabled)
                return RedirectToRoute("CheckoutOnePage");

            if (_workContext.CurrentCustomer.IsGuest() && !_orderSettings.AnonymousCheckoutAllowed)
                return Challenge();

            //Check whether payment workflow is required
            var isPaymentWorkflowRequired = _orderProcessingService.IsPaymentWorkflowRequired(cart);
            if (!isPaymentWorkflowRequired)
            {
                return RedirectToRoute("CheckoutConfirm");
            }

            //load payment method
            var paymentMethodSystemName = _genericAttributeService.GetAttribute<string>(_workContext.CurrentCustomer,
                NopCustomerDefaults.SelectedPaymentMethodAttribute, _storeContext.CurrentStore.Id);
            var paymentMethod = _paymentService.LoadPaymentMethodBySystemName(paymentMethodSystemName);
            if (paymentMethod == null)
                return RedirectToRoute("CheckoutPaymentMethod");

            HelperParatikaService.ParatikaParameter(requestParameters, cart, _workContext, _httpContextAccessor, _webHelper, _paratikaOrderPaymentSettings, _orderTotalCalculationService);
            var getSessionToken = _session.Get<string>("SESSIONTOKEN_" + _workContext.CurrentCustomer.Id);
            requestParameters.Add("SESSIONTOKEN", getSessionToken.ToString());
            var requestData = HelperParatikaService.convertToRequestData(requestParameters);
            //var response = HelperParatikaService.getConnection(_paratikaOrderPaymentSettings.URL + "post/sale3d/" + getSessionToken.ToString(), requestData);

            //redirect url - 3D Security
            return Json(new { url = _paratikaOrderPaymentSettings.URL + "post/sale3d/" + getSessionToken.ToString(), data = requestData });
        }

        public IActionResult PaymentInfoCallBack(IFormCollection form)
        {
            // this redirect was performed because _workcontext.currentuser is empty.
            return RedirectToAction("ConfirmParatikaPayment", new { merchantPaymentId = form["merchantPaymentId"], responseCode = form["responseCode"], responseMsg = form["responseMsg"] });
        }

        public IActionResult ConfirmParatikaPayment(string merchantPaymentId, string responseCode, string responseMsg)
        {
            var model = new PaymentInfoModel();
            var processPaymentRequest = new ProcessPaymentRequest();
            processPaymentRequest.OrderGuid = Guid.Parse(merchantPaymentId);
            HttpContext.Session.Set<ProcessPaymentRequest>("OrderPaymentInfo", processPaymentRequest);

            if (_orderSettings.CheckoutDisabled)
                return RedirectToRoute("ShoppingCart");

            var cart = _workContext.CurrentCustomer.ShoppingCartItems
                .Where(sci => sci.ShoppingCartType == ShoppingCartType.ShoppingCart)
                .LimitPerStore(_storeContext.CurrentStore.Id)
                .ToList();
            if (!cart.Any())
                return RedirectToRoute("ShoppingCart");

            if (_orderSettings.OnePageCheckoutEnabled)
                return RedirectToRoute("CheckoutOnePage");

            if (_workContext.CurrentCustomer.IsGuest() && !_orderSettings.AnonymousCheckoutAllowed)
                return Challenge();
            //Check whether payment workflow is required
            var isPaymentWorkflowRequired = _orderProcessingService.IsPaymentWorkflowRequired(cart);
            if (!isPaymentWorkflowRequired)
            {
                return RedirectToRoute("CheckoutConfirm");
            }

            //load payment method
            var paymentMethodSystemName = _genericAttributeService.GetAttribute<string>(_workContext.CurrentCustomer,
                NopCustomerDefaults.SelectedPaymentMethodAttribute, _storeContext.CurrentStore.Id);
            var paymentMethod = _paymentService.LoadPaymentMethodBySystemName(paymentMethodSystemName);
            if (paymentMethod == null)
                return RedirectToRoute("CheckoutPaymentMethod");

            var orderGuid = _session.Get<string>("MERCHANTPAYMENTID_" + _workContext.CurrentCustomer.Id);

            if (responseCode == "00")
            {
                try
                {
                    processPaymentRequest.StoreId = _storeContext.CurrentStore.Id;
                    processPaymentRequest.CustomerId = _workContext.CurrentCustomer.Id;
                    processPaymentRequest.PaymentMethodSystemName = _genericAttributeService.GetAttribute<string>(_workContext.CurrentCustomer,
                        NopCustomerDefaults.SelectedPaymentMethodAttribute, _storeContext.CurrentStore.Id);
                    processPaymentRequest.OrderGuid = Guid.Parse(merchantPaymentId);
                    HttpContext.Session.Set<ProcessPaymentRequest>("OrderPaymentInfo", processPaymentRequest);
                    var placeOrderResult = _orderProcessingService.PlaceOrder(processPaymentRequest);
                    if (placeOrderResult.Success)
                    {
                        HttpContext.Session.Set<ProcessPaymentRequest>("OrderPaymentInfo", null);
                        var postProcessPaymentRequest = new PostProcessPaymentRequest
                        {
                            Order = placeOrderResult.PlacedOrder,
                        };
                        _paymentService.PostProcessPayment(postProcessPaymentRequest);

                        if (_webHelper.IsRequestBeingRedirected || _webHelper.IsPostBeingDone)
                        {
                            //redirection or POST has been done in PostProcessPayment
                            return Content("Redirected");
                        }

                        return RedirectToRoute("CheckoutCompleted", new { orderId = placeOrderResult.PlacedOrder.Id });
                    }

                    foreach (var error in placeOrderResult.Errors)
                        model.Error += error;
                }
                catch (Exception exc)
                {
                    _logger.Warning(exc.Message, exc);
                    model.Error += exc.Message;
                }
            }
            else
            {
                model.Error += _localizationService.GetResource("Plugins.Payments.Paratika.Fields.Error");
            }

            model.Error += responseMsg;
            return RedirectToRoute("CheckoutPaymentMethod");
        }

        [HttpGet]
        public IActionResult ParatikaInstallment(string cardpin)
        {
            //validation
            var model = new PaymentInfoModel();
            Dictionary<String, String> requestParameters = new Dictionary<String, String>();

            var cart = _workContext.CurrentCustomer.ShoppingCartItems
                .Where(sci => sci.ShoppingCartType == ShoppingCartType.ShoppingCart)
                .LimitPerStore(_storeContext.CurrentStore.Id)
                .ToList();
            if (!cart.Any())
                return RedirectToRoute("ShoppingCart");

            var orderTotal = _orderTotalCalculationService.GetShoppingCartTotal(cart, out var orderDiscountAmount, out var orderAppliedDiscounts, out var appliedGiftCards, out var redeemedRewardPoints, out var redeemedRewardPointsAmount);

            var getSessionToken = _session.Get<string>("SESSIONTOKEN_" + _workContext.CurrentCustomer.Id);
            requestParameters.Add("MERCHANT", _paratikaOrderPaymentSettings.Code);
            requestParameters.Add("MERCHANTUSER", _paratikaOrderPaymentSettings.Username);
            requestParameters.Add("MERCHANTPASSWORD", _paratikaOrderPaymentSettings.Password);
            requestParameters.Add("ACTION", "QUERYPAYMENTSYSTEMS");
            requestParameters.Add("SESSIONTOKEN", getSessionToken.ToString());
            requestParameters.Add("BIN", cardpin);

            var requestData = HelperParatikaService.convertToRequestData(requestParameters);
            var response = HelperParatikaService.getConnection(_paratikaOrderPaymentSettings.URL, requestData);

            var getResponseInstallment = Newtonsoft.Json.JsonConvert.DeserializeObject<InstallmentsDetail>(response);
            if (getResponseInstallment == null)
                getResponseInstallment = new InstallmentsDetail();
            if (getResponseInstallment.installmentPaymentSystem == null)
                getResponseInstallment.installmentPaymentSystem = new InstallmentPaymentSystem();
            if (getResponseInstallment.installmentPaymentSystem.installmentList == null)
                getResponseInstallment.installmentPaymentSystem.installmentList = new List<InstallmentList>();

            getResponseInstallment.installmentPaymentSystem.installmentList.Add(new InstallmentList() { count = "1", customerCostCommissionRate = 1, customerCostCommissionPrice = orderTotal.Value * 1 });

            getResponseInstallment.installmentPaymentSystem.installmentList.ForEach(x => x.customerCostCommissionPrice = orderTotal.Value * x.customerCostCommissionRate);
            getResponseInstallment.installmentPaymentSystem.currencyCode = _workContext.WorkingCurrency.CurrencyCode;
            getResponseInstallment.installmentPaymentSystem.displayLocales = _workContext.WorkingCurrency.DisplayLocale;

            return Json(getResponseInstallment);
        }
    }
}
