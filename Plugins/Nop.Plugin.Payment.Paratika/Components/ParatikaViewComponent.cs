using Castle.Core.Logging;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
using Nop.Core;
using Nop.Core.Domain.Directory;
using Nop.Core.Domain.Orders;
using Nop.Core.Http.Extensions;
using Nop.Plugin.Payment.Paratika.Helpers;
using Nop.Plugin.Payment.Paratika.Models;
using Nop.Services.Catalog;
using Nop.Services.Common;
using Nop.Services.Configuration;
using Nop.Services.Customers;
using Nop.Services.Directory;
using Nop.Services.Localization;
using Nop.Services.Orders;
using Nop.Services.Payments;
using Nop.Services.Security;
using Nop.Services.Stores;
using Nop.Web.Framework.Components;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Web;

namespace Nop.Plugin.Payment.Paratika.Components
{
    [ViewComponent(Name = "Paratika")]
    public class ParatikaViewComponent : NopViewComponent
    {
        private readonly IWorkContext _workContext;
        private readonly IStoreContext _storeContext;
        private readonly ParatikaOrderPaymentSettings _paratikaOrderPaymentSettings;
        private readonly IOrderTotalCalculationService _orderTotalCalculationService;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IWebHelper _webHelper;


        private ISession _session => _httpContextAccessor.HttpContext.Session;

        public ParatikaViewComponent(IWorkContext workContext,
            IHttpContextAccessor httpContextAccessor,
            IStoreContext storeContext,
            ParatikaOrderPaymentSettings ParatikaOrderPaymentSettings,
            IWebHelper webHelper,
            IOrderTotalCalculationService orderTotalCalculationService
            )
        {
            this._httpContextAccessor = httpContextAccessor;
            this._workContext = workContext;
            this._storeContext = storeContext;
            this._webHelper = webHelper;
            this._paratikaOrderPaymentSettings = ParatikaOrderPaymentSettings;
            this._orderTotalCalculationService = orderTotalCalculationService;
        }

        public IViewComponentResult Invoke()
        {
            var model = new PaymentInfoModel();
            if (this.Request.Method != WebRequestMethods.Http.Get)
            {
                var form = this.Request.Form;
                model.CardholderName = form["CardholderName"];
                model.CardNumber = form["CardNumber"];
                model.CardCode = form["CardCode"];
                model.ExpireCardMounth = form["ExpireCardMounth"];
                model.ExpireCardYear = form["ExpireCardYear"];
            }
            model.IsProcess3D = true;
            return ParatikaPaymentInfo(model);
        }

        public IViewComponentResult ParatikaPaymentInfo(PaymentInfoModel model)
        {
            Dictionary<String, String> requestParameters = new Dictionary<String, String>();

            var processGuid = Guid.NewGuid().ToString();
            HttpContext.Session.Set("MERCHANTPAYMENTID_" + _workContext.CurrentCustomer.Id, processGuid);



            var cart = _workContext.CurrentCustomer.ShoppingCartItems
                .Where(sci => sci.ShoppingCartType == ShoppingCartType.ShoppingCart)
                .LimitPerStore(_storeContext.CurrentStore.Id)
                .ToList();

            requestParameters.Add("ACTION", "SESSIONTOKEN");

            HelperParatikaService.ParatikaParameter(requestParameters, cart,_workContext,_httpContextAccessor,_webHelper,_paratikaOrderPaymentSettings,_orderTotalCalculationService);

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
                model.Error = paratikaResponse.GetValue("errorCode").ToString() + "-" + paratikaResponse.GetValue("errorMsg").ToString();
                model.Error += "Lütfen sayfayı yenileyerek tekrar deneyiniz!!";
                return View("~/Plugins/Payments.Paratika/Views/PaymentInfo.cshtml", model);
            }
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
        //    }
        //    catch (Exception e)
        //    {
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
        //    }
        //    return encodedValue;
        //}

    }
}
