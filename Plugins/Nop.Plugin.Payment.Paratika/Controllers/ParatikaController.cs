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

namespace Nop.Plugin.Payment.Paratika.Controllers
{
    [AuthorizeAdmin]
    [Area(AreaNames.Admin)]
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
            //AccountIyzico.AccountIyzicoInfo(settingService);
        }

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
            model.Guid = orderPaymentSettings.Guid;
            model.AdditionalFee = orderPaymentSettings.AdditionalFee;
            model.AdditionalFeePercentage = orderPaymentSettings.AdditionalFeePercentage;
            model.URL = orderPaymentSettings.URL;
            model.ActiveStoreScopeConfiguration = storeScope;
            if (storeScope > 0)
            {
                model.Password_OverrideForStore = _settingService.SettingExists(orderPaymentSettings,
                    x => x.Password, storeScope);
                model.Code_OverrideForStore = _settingService.SettingExists(orderPaymentSettings, x => x.Code,
                    storeScope);
                model.Username_OverrideForStore = _settingService.SettingExists(orderPaymentSettings,
                    x => x.Username, storeScope);
                model.Guid_OverrideForStore = _settingService.SettingExists(orderPaymentSettings,
                    x => x.Guid, storeScope);
                model.URL_OverrideForStore = _settingService.SettingExists(orderPaymentSettings,
                  x => x.URL, storeScope);
                //model.Signature_OverrideForStore = _settingService.SettingExists(iyzicoOrderPaymentSettings, x => x.Signature, storeScope);
                model.AdditionalFee_OverrideForStore = _settingService.SettingExists(orderPaymentSettings,
                    x => x.AdditionalFee, storeScope);
                model.AdditionalFeePercentage_OverrideForStore =
                    _settingService.SettingExists(orderPaymentSettings, x => x.AdditionalFeePercentage, storeScope);
            }
            return View("~/Plugins/Payments.Paratika/Views/Configure.cshtml", model);
        }

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
            paratikaSettings.Guid = model.Guid;
            paratikaSettings.Code = model.Code;
            paratikaSettings.AdditionalFee = model.AdditionalFee;
            paratikaSettings.AdditionalFeePercentage = model.AdditionalFeePercentage;
            paratikaSettings.URL = model.URL;
            _settingService.SaveSettingOverridablePerStore(paratikaSettings, x => x.Password, model.Password_OverrideForStore, storeScope, false);
            _settingService.SaveSettingOverridablePerStore(paratikaSettings, x => x.Code, model.Code_OverrideForStore, storeScope, false);
            _settingService.SaveSettingOverridablePerStore(paratikaSettings, x => x.Username, model.Username_OverrideForStore, storeScope, false);
            _settingService.SaveSettingOverridablePerStore(paratikaSettings, x => x.URL, model.URL_OverrideForStore, storeScope, false);
            _settingService.SaveSettingOverridablePerStore(paratikaSettings, x => x.AdditionalFee, model.AdditionalFee_OverrideForStore, storeScope, false);
            _settingService.SaveSettingOverridablePerStore(paratikaSettings, x => x.AdditionalFeePercentage, model.AdditionalFeePercentage_OverrideForStore, storeScope, false);
            _settingService.SaveSettingOverridablePerStore(paratikaSettings, x => x.Guid, model.Guid_OverrideForStore, storeScope, false);
            _settingService.ClearCache();

            SuccessNotification(_localizationService.GetResource("Admin.Plugins.Saved"));

            return Configure();
        }

        [HttpGet]
        public IActionResult ParatikaPaymentInfo(PaymentInfoModel model)
        {
            Dictionary<String, String> requestParameters = new Dictionary<String, String>();

            var processGuid = Guid.NewGuid().ToString();
            HttpContext.Session.Set("MERCHANTPAYMENTID_" + _workContext.CurrentCustomer.Id, processGuid);

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
            requestParameters.Add("ACTION", "SESSIONTOKEN");

            HelperParatikaService.ParatikaParameter(requestParameters, cart, _workContext, _httpContextAccessor, _webHelper, _paratikaOrderPaymentSettings, _orderTotalCalculationService);

            var requestData = HelperParatikaService.convertToRequestData(requestParameters);
            var response = HelperParatikaService.getConnection(_paratikaOrderPaymentSettings.URL, requestData);
            //var response = getConnection(_paratikaOrderPaymentSettings.URL, requestData);
            JObject paratikaResponse = JObject.Parse(response);
            if (
                response != null
                && "00".Equals(paratikaResponse.GetValue("responseCode").ToString(), StringComparison.InvariantCultureIgnoreCase)
                && "Approved".Equals(paratikaResponse.GetValue("responseMsg").ToString(), StringComparison.InvariantCultureIgnoreCase)
               )
            {
                model.SessionToken = paratikaResponse["sessionToken"].ToString();
                HttpContext.Session.Set("SESSIONTOKEN_" + _workContext.CurrentCustomer.Id, paratikaResponse["sessionToken"].ToString());
                return View("~/Plugins/Payments.Paratika/Views/PaymentInfo.cshtml", model);
            }
            else
            {
                _logger.Information(" > SESSIONTOKEN operation is FAILED, please check the error codes.");
                _logger.Information("   ErrorCode     : " + paratikaResponse.GetValue("errorCode"));
                _logger.Information("   ErrorMessage  : " + paratikaResponse.GetValue("errorMsg"));
                model.Error = paratikaResponse.GetValue("errorCode").ToString() + "-" + paratikaResponse.GetValue("errorMsg").ToString();
                model.Error = "Lütfen sayfayı yenileyerek tekrar deneyiniz!!";
                //model.IsError = true;
                //model.Error = paratikaResponse.GetValue("errorCode").ToString() + " - " + paratikaResponse.GetValue("errorMsg").ToString();
                return View("~/Plugins/Payments.Paratika/Views/PaymentInfo.cshtml", model);
            }
        }

        [HttpGet]
        public IActionResult ParatikaPaymentInfo3D(PaymentInfoModel model)
        {
            Dictionary<String, String> requestParameters = new Dictionary<String, String>();

            requestParameters.Add("NAMEONCARD", model.CardholderName);
            requestParameters.Add("CARDPAN", model.CardNumber);
            requestParameters.Add("CARDEXPIRY", model.ExpireCardMounth + "." + model.ExpireCardYear.Substring(2,2).ToString());
            requestParameters.Add("CARDCVV", model.CardCode);
            requestParameters.Add("ACTION", "SALE");
            requestParameters.Add("INSTALLMENTS", model.Installment.ToString());

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

            var requestData = HelperParatikaService.convertToRequestData(requestParameters);
            var getSessionToken = HttpContext.Session.Get<string>("SESSIONTOKEN_" + _workContext.CurrentCustomer.Id);
            requestParameters.Add("SESSIONTOKEN", getSessionToken.ToString());
            var response = HelperParatikaService.getConnection(_paratikaOrderPaymentSettings.URL, requestData);
            if (response.Contains("<!DOCTYPE"))
            {
                return Content(response);
            }
            else
            {
                model.Error = "Güvenli ödemeye bağlanılamadı. Tekrar edeneyiniz!";
            }
            return View("~/Plugins/Payments.Paratika/Views/PaymentInfo.cshtml", model);
        }

        [HttpGet]
        public IActionResult PaymentInfoCallBack(SaleResponse saleResponse)
        {
            //validation
            var model = new PaymentInfoModel();
            if (_orderSettings.CheckoutDisabled)
                return RedirectToRoute("ShoppingCart");

            Dictionary<String, String> requestParameters = new Dictionary<String, String>();
            //Paratika Validation
            requestParameters.Add("MERCHANT", _paratikaOrderPaymentSettings.Code);
            requestParameters.Add("MERCHANTUSER", _paratikaOrderPaymentSettings.Username);
            requestParameters.Add("MERCHANTPASSWORD", _paratikaOrderPaymentSettings.Password);
            requestParameters.Add("ACTION", "QUERYTRANSACTION");
            requestParameters.Add("PGTRANID", saleResponse.pgTranId);

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

            var requestData = HelperParatikaService.convertToRequestData(requestParameters);
            var getSessionToken = HttpContext.Session.Get<string>("SESSIONTOKEN_" + _workContext.CurrentCustomer.Id);

            var response = HelperParatikaService.getConnection(_paratikaOrderPaymentSettings.URL, requestData);

            var orderGuid = HttpContext.Session.Get<string>("MERCHANTPAYMENTID_" + _workContext.CurrentCustomer.Id);

            if (saleResponse.responseCode == "00" && saleResponse.merchantPaymentId == orderGuid)
            {
                //model
                try
                {
                    var responseDictionaryType = saleResponse.GetType().GetProperties(BindingFlags.Instance | BindingFlags.Public).ToDictionary(prop => prop.Name, prop => prop.GetValue(saleResponse, null));

                    var processPaymentRequest = new ProcessPaymentRequest();
                    processPaymentRequest.StoreId = _storeContext.CurrentStore.Id;
                    processPaymentRequest.CustomerId = _workContext.CurrentCustomer.Id;
                    processPaymentRequest.PaymentMethodSystemName = _genericAttributeService.GetAttribute<string>(_workContext.CurrentCustomer,
                        NopCustomerDefaults.SelectedPaymentMethodAttribute, _storeContext.CurrentStore.Id);
                    processPaymentRequest.OrderGuid = Guid.Parse(orderGuid);
                    processPaymentRequest.CustomValues = responseDictionaryType;
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
                if (saleResponse.merchantPaymentId != orderGuid)
                {
                    model.Error += "MerchantPaymentId hatası oluştu. Lütfen site yöneticisine durumu bildiriniz.";
                }
            }

            model.Error += saleResponse.responseMsg;
            return View("~/Plugins/Payments.Paratika/Views/PaymentInfo.cshtml", model);
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

            var getSessionToken = HttpContext.Session.Get<string>("SESSIONTOKEN_" + _workContext.CurrentCustomer.Id);
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

        //private void ParatikaParameter(Dictionary<string, string> requestParameters, List<ShoppingCartItem> cart)
        //{
        //    var storeLocation = _webHelper.GetStoreLocation();
        //    var getPaymentGuid = HttpContext.Session.Get<string>("MERCHANTPAYMENTID_" + _workContext.CurrentCustomer.Id);

        //    requestParameters.Add("MERCHANT", _paratikaOrderPaymentSettings.Code);
        //    requestParameters.Add("MERCHANTUSER", _paratikaOrderPaymentSettings.Username);
        //    requestParameters.Add("MERCHANTPASSWORD", _paratikaOrderPaymentSettings.Password);

        //    requestParameters.Add("MERCHANTPAYMENTID", getPaymentGuid);
        //    var orderTotal = _orderTotalCalculationService.GetShoppingCartTotal(cart, out var orderDiscountAmount, out var orderAppliedDiscounts, out var appliedGiftCards, out var redeemedRewardPoints, out var redeemedRewardPointsAmount);
        //    requestParameters.Add("AMOUNT", orderTotal.ToString());
        //    requestParameters.Add("CURRENCY", "TRY");
        //    requestParameters.Add("SESSIONTYPE", "PAYMENTSESSION");
        //    requestParameters.Add("RETURNURL", $"{storeLocation}/Admin/Paratika/PaymentInfoCallBack");

        //    JArray oItems = new JArray();
        //    foreach (var card in cart)
        //    {
        //        JObject item = new JObject();
        //        item.Add("code", card.Product.Sku);
        //        item.Add("name", card.Product.Name);
        //        item.Add("quantity", card.Quantity);
        //        item.Add("description", card.Product.Name);
        //        item.Add("amount", card.Product.Price);
        //        oItems.Add(item);
        //    }
        //    var productSum = cart.Sum(x => x.Product.Price);
        //    if (orderTotal != productSum)
        //    {
        //        if (orderTotal > productSum)
        //        {
        //            var diffrerent = orderTotal - productSum;
        //            JObject item = new JObject();
        //            item.Add("code", "vergi");
        //            item.Add("name", "vergi");
        //            item.Add("quantity", "1");
        //            item.Add("description", "vergi");
        //            item.Add("amount", diffrerent.ToString());
        //            oItems.Add(item);
        //        }
        //        else
        //        {
        //            var diffrerent = productSum - orderTotal;
        //            JObject item = new JObject();
        //            item.Add("code", "indirim");
        //            item.Add("name", "indirim");
        //            item.Add("quantity", "1");
        //            item.Add("description", "indirim");
        //            item.Add("amount", diffrerent.ToString());
        //            oItems.Add(item);
        //        }
        //    }
        //    requestParameters.Add("ORDERITEMS", encodeParameter(oItems.ToString()));

        //    JObject extra = new JObject();
        //    extra.Add("IntegrationModel", "API");
        //    extra.Add("AlwaysSaveCard", "true");
        //    requestParameters.Add("EXTRA", encodeParameter(extra.ToString()));

        //    requestParameters.Add("CUSTOMER", _workContext.CurrentCustomer.CustomerGuid.ToString());
        //    requestParameters.Add("CUSTOMERNAME", _workContext.CurrentCustomer.ShippingAddress.FirstName + " " + _workContext.CurrentCustomer.ShippingAddress.LastName);
        //    requestParameters.Add("CUSTOMEREMAIL", _workContext.CurrentCustomer.Email);
        //    requestParameters.Add("CUSTOMERIP", _workContext.CurrentCustomer.LastIpAddress);
        //    requestParameters.Add("CUSTOMERPHONE", _workContext.CurrentCustomer.ShippingAddress.PhoneNumber);
        //    //requestParameters.Add("CUSTOMERBIRTHDAY", _workContext.CurrentCustomer.ShippingAddress.);
        //    requestParameters.Add("CUSTOMERUSERAGENT", _httpContextAccessor.HttpContext.Request.Headers["User-Agent"].ToString());

        //    requestParameters.Add("BILLTOADDRESSLINE", _workContext.CurrentCustomer.BillingAddress.Address1);
        //    requestParameters.Add("BILLTOCITY", _workContext.CurrentCustomer.BillingAddress.City);
        //    requestParameters.Add("BILLTOCOUNTRY", _workContext.CurrentCustomer.BillingAddress.Country.Name);
        //    requestParameters.Add("BILLTOPOSTALCODE", _workContext.CurrentCustomer.BillingAddress.ZipPostalCode);
        //    requestParameters.Add("BILLTOPHONE", _workContext.CurrentCustomer.BillingAddress.PhoneNumber);
        //    requestParameters.Add("SHIPTOADDRESSLINE", _workContext.CurrentCustomer.ShippingAddress.Address1);
        //    requestParameters.Add("SHIPTOCITY", _workContext.CurrentCustomer.ShippingAddress.City);
        //    requestParameters.Add("SHIPTOCOUNTRY", _workContext.CurrentCustomer.ShippingAddress.Country.Name);
        //    requestParameters.Add("SHIPTOPOSTALCODE", _workContext.CurrentCustomer.ShippingAddress.ZipPostalCode);
        //    requestParameters.Add("SHIPTOPHONE", _workContext.CurrentCustomer.ShippingAddress.PhoneNumber);
        //}

        //private void checkResponse(JObject json, string response, string action)
        //{
        //    if (response != null
        //        && "00".Equals(json.GetValue("responseCode").ToString(), StringComparison.InvariantCultureIgnoreCase)
        //        && "Approved".Equals(json.GetValue("responseMsg").ToString(), StringComparison.InvariantCultureIgnoreCase))
        //    {
        //        _logger.Information(" > " + action + " operation is SUCCESSFUL.");
        //    }
        //    else
        //    {
        //        _logger.Information(" > " + action + " operation is FAILED, please check the error codes.");
        //        _logger.Information("   ErrorCode     : " + json.GetValue("errorCode"));
        //        _logger.Information("   ErrorMessage  : " + json.GetValue("errorMsg"));
        //    }
        //}

        //private string getConnection(string url, string reqMsg)
        //{
        //    string outputData = string.Empty;
        //    try
        //    {
        //        HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
        //        request.Method = "POST";
        //        request.ContentType = "application/x-www-form-urlencoded";
        //        ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls | SecurityProtocolType.Tls11;
        //        ServicePointManager.ServerCertificateValidationCallback = delegate { return true; };
        //        System.Net.ServicePointManager.Expect100Continue = false;
        //        byte[] data = Encoding.UTF8.GetBytes(reqMsg);
        //        using (Stream stream = request.GetRequestStream())
        //        {
        //            stream.Write(data, 0, data.Length);
        //        }
        //        request.KeepAlive = false;
        //        HttpWebResponse response =
        //        (HttpWebResponse)request.GetResponse();
        //        Stream streamResponse = response.GetResponseStream();
        //        StreamReader streamRead = new StreamReader(streamResponse);
        //        string read = streamRead.ReadToEnd();
        //        outputData = read;
        //        streamResponse.Close();
        //        streamRead.Close();
        //        response.Close();

        //    }
        //    catch (WebException e)
        //    {
        //        _logger.Information(" ParatikaService --> getConnection(url,reqMsg) --> WebException " + e.ToString());
        //    }
        //    catch (Exception e)
        //    {
        //        _logger.Information(" ParatikaService --> getConnection(url,reqMsg) --> Exception " + e.ToString());
        //    }

        //    return outputData;
        //}

        //private string convertToRequestData(Dictionary<string, string> requestParameters)
        //{
        //    StringBuilder requestData = new StringBuilder();
        //    foreach (KeyValuePair<string, string> entry in requestParameters)
        //    {
        //        var key = HttpUtility.UrlEncode(entry.Key);
        //        var value = HttpUtility.UrlEncode(entry.Value);
        //        requestData.Append(key + "=" + value + "&");
        //    }
        //    return requestData.ToString();
        //}

        //private string encodeParameter(string parameterValue)
        //{
        //    string encodedValue = string.Empty;
        //    try
        //    {
        //        if (parameterValue != null)
        //        {
        //            encodedValue = HttpUtility.UrlEncode(parameterValue);
        //        }
        //    }
        //    catch
        //    {
        //        _logger.Information(" ParatikaUtil --> encodeParameter(parameterValue) --> UnsupportedEncodingException ");
        //    }
        //    return encodedValue;
        //}
    }
}
