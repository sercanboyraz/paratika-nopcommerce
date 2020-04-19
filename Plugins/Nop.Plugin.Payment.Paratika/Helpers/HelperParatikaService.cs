using Microsoft.AspNetCore.Http;
using Newtonsoft.Json.Linq;
using Nop.Core;
using Nop.Core.Domain.Orders;
using Nop.Core.Http.Extensions;
using Nop.Services.Orders;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Web;

namespace Nop.Plugin.Payment.Paratika.Helpers
{
    public static class HelperParatikaService
    {
        public static void ParatikaParameter(Dictionary<string, string> requestParameters, List<ShoppingCartItem> cart, IWorkContext _workContext, IHttpContextAccessor _httpContextAccessor, IWebHelper _webHelper, ParatikaOrderPaymentSettings _paratikaOrderPaymentSettings, IOrderTotalCalculationService _orderTotalCalculationService)
        {
            var storeLocation = _webHelper.GetStoreLocation();
            var getPaymentGuid = _httpContextAccessor.HttpContext.Session.Get<string>("MERCHANTPAYMENTID_" + _workContext.CurrentCustomer.Id);

            requestParameters.Add("MERCHANT", _paratikaOrderPaymentSettings.Code);
            requestParameters.Add("MERCHANTUSER", _paratikaOrderPaymentSettings.Username);
            requestParameters.Add("MERCHANTPASSWORD", _paratikaOrderPaymentSettings.Password);

            requestParameters.Add("MERCHANTPAYMENTID", getPaymentGuid);
            var orderTotal = _orderTotalCalculationService.GetShoppingCartTotal(cart, out var orderDiscountAmount, out var orderAppliedDiscounts, out var appliedGiftCards, out var redeemedRewardPoints, out var redeemedRewardPointsAmount);
            requestParameters.Add("AMOUNT", orderTotal.ToString());
            requestParameters.Add("CURRENCY", "TRY");
            requestParameters.Add("SESSIONTYPE", "PAYMENTSESSION");
            requestParameters.Add("RETURNURL", $"{storeLocation}Paratika/PaymentInfoCallBack");

            JArray oItems = new JArray();
            foreach (var card in cart)
            {
                JObject item = new JObject();
                item.Add("code", card.Product.Sku);
                item.Add("name", card.Product.Name);
                item.Add("quantity", card.Quantity);
                item.Add("description", card.Product.Name);
                item.Add("amount", card.Product.Price);
                oItems.Add(item);
            }

            decimal productSum = 0;
            foreach (var item in oItems)
            {
                productSum += Convert.ToDecimal(item["amount"]) * Convert.ToInt32(item["quantity"]);
            }
            if (orderTotal != productSum)
            {
                if (orderTotal > productSum)
                {
                    var diffrerent = orderTotal - productSum;
                    JObject item = new JObject();
                    item.Add("code", "different");
                    item.Add("name", "different");
                    item.Add("quantity", "1");
                    item.Add("description", "different");
                    item.Add("amount", diffrerent.ToString());
                    oItems.Add(item);
                }
                else
                {
                    var diffrerent = productSum - orderTotal;
                    JObject item = new JObject();
                    item.Add("code", "discount");
                    item.Add("name", "discount");
                    item.Add("quantity", "1");
                    item.Add("description", "discount");
                    item.Add("amount", diffrerent.ToString());
                    oItems.Add(item);
                }
            }
            requestParameters.Add("ORDERITEMS", encodeParameter(oItems.ToString()));

            JObject extra = new JObject();
            extra.Add("IntegrationModel", "API");
            //extra.Add("AlwaysSaveCard", "true");
            requestParameters.Add("EXTRA", encodeParameter(extra.ToString()));

            requestParameters.Add("CUSTOMER", _workContext.CurrentCustomer.CustomerGuid.ToString());
            requestParameters.Add("CUSTOMERNAME", _workContext.CurrentCustomer.ShippingAddress.FirstName + " " + _workContext.CurrentCustomer.ShippingAddress.LastName);
            requestParameters.Add("CUSTOMEREMAIL", _workContext.CurrentCustomer.Email);
            requestParameters.Add("CUSTOMERIP", _workContext.CurrentCustomer.LastIpAddress);
            requestParameters.Add("CUSTOMERPHONE", _workContext.CurrentCustomer.ShippingAddress.PhoneNumber);
            //requestParameters.Add("CUSTOMERBIRTHDAY", _workContext.CurrentCustomer.ShippingAddress.);
            requestParameters.Add("CUSTOMERUSERAGENT", _httpContextAccessor.HttpContext.Request.Headers["User-Agent"].ToString());

            requestParameters.Add("BILLTOADDRESSLINE", _workContext.CurrentCustomer.BillingAddress.Address1);
            requestParameters.Add("BILLTOCITY", _workContext.CurrentCustomer.BillingAddress.City);
            requestParameters.Add("BILLTOCOUNTRY", _workContext.CurrentCustomer.BillingAddress.Country.Name);
            requestParameters.Add("BILLTOPOSTALCODE", _workContext.CurrentCustomer.BillingAddress.ZipPostalCode);
            requestParameters.Add("BILLTOPHONE", _workContext.CurrentCustomer.BillingAddress.PhoneNumber);
            requestParameters.Add("SHIPTOADDRESSLINE", _workContext.CurrentCustomer.ShippingAddress.Address1);
            requestParameters.Add("SHIPTOCITY", _workContext.CurrentCustomer.ShippingAddress.City);
            requestParameters.Add("SHIPTOCOUNTRY", _workContext.CurrentCustomer.ShippingAddress.Country.Name);
            requestParameters.Add("SHIPTOPOSTALCODE", _workContext.CurrentCustomer.ShippingAddress.ZipPostalCode);
            requestParameters.Add("SHIPTOPHONE", _workContext.CurrentCustomer.ShippingAddress.PhoneNumber);
        }


        public static string getConnection(string url, string reqMsg)
        {
            string outputData = string.Empty;

            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
            request.Method = "POST";
            request.ContentType = "application/x-www-form-urlencoded";
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls | SecurityProtocolType.Tls11;
            ServicePointManager.ServerCertificateValidationCallback = delegate { return true; };
            System.Net.ServicePointManager.Expect100Continue = false;
            byte[] data = Encoding.UTF8.GetBytes(reqMsg);
            using (Stream stream = request.GetRequestStream())
            {
                stream.Write(data, 0, data.Length);
            }
            request.KeepAlive = false;
            HttpWebResponse response =
            (HttpWebResponse)request.GetResponse();
            Stream streamResponse = response.GetResponseStream();
            StreamReader streamRead = new StreamReader(streamResponse);
            string read = streamRead.ReadToEnd();
            outputData = read;
            streamResponse.Close();
            streamRead.Close();
            response.Close();

            return outputData;
        }

        public static string convertToRequestData(Dictionary<string, string> requestParameters)
        {
            StringBuilder requestData = new StringBuilder();
            foreach (KeyValuePair<string, string> entry in requestParameters)
            {
                var key = HttpUtility.UrlEncode(entry.Key);
                var value = HttpUtility.UrlEncode(entry.Value);
                requestData.Append(key + "=" + value + "&");
            }
            return requestData.ToString();
        }

        public static string encodeParameter(string parameterValue)
        {
            string encodedValue = string.Empty;
            if (parameterValue != null)
            {
                encodedValue = HttpUtility.UrlEncode(parameterValue);
            }
            return encodedValue;
        }
    }
}
