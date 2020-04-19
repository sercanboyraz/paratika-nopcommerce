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
using Nop.Core.Http.Extensions;

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
        private readonly ILocalizationService _localizationService;


        private ISession _session => _httpContextAccessor.HttpContext.Session;

        public ParatikaViewComponent(IWorkContext workContext,
            IHttpContextAccessor httpContextAccessor,
            IStoreContext storeContext,
            ILocalizationService localizationService,
            ParatikaOrderPaymentSettings ParatikaOrderPaymentSettings,
            IWebHelper webHelper,
            IOrderTotalCalculationService orderTotalCalculationService
            )
        {
            this._httpContextAccessor = httpContextAccessor;
            this._workContext = workContext;
            this._storeContext = storeContext;
            this._webHelper = webHelper;
            this._localizationService = localizationService;
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
            return ParatikaPaymentInfo(model);
        }

        public IViewComponentResult ParatikaPaymentInfo(PaymentInfoModel model)
        {
            Dictionary<String, String> requestParameters = new Dictionary<String, String>();

            requestParameters.Add("ACTION", "SESSIONTOKEN");

            var processGuid = Guid.NewGuid().ToString();
            _session.Set<string>("MERCHANTPAYMENTID_" + _workContext.CurrentCustomer.Id, processGuid);

            var cart = _workContext.CurrentCustomer.ShoppingCartItems
                .Where(sci => sci.ShoppingCartType == ShoppingCartType.ShoppingCart)
                .LimitPerStore(_storeContext.CurrentStore.Id)
                .ToList();
            if (!cart.Any())
            {
                model.Error = _localizationService.GetResource("Plugins.Payments.Paratika.Fields.Error");
                return View("~/Plugins/Payments.Paratika/Views/PaymentInfo.cshtml", model);
            }

            HelperParatikaService.ParatikaParameter(requestParameters, cart, _workContext, _httpContextAccessor, _webHelper, _paratikaOrderPaymentSettings, _orderTotalCalculationService);
            var requestData = HelperParatikaService.convertToRequestData(requestParameters);
            var response = HelperParatikaService.getConnection(_paratikaOrderPaymentSettings.URL, requestData);
            JObject paratikaResponse = JObject.Parse(response);
            if (
                response != null
                && "00".Equals(paratikaResponse.GetValue("responseCode").ToString(), StringComparison.InvariantCultureIgnoreCase)
                && "Approved".Equals(paratikaResponse.GetValue("responseMsg").ToString(), StringComparison.InvariantCultureIgnoreCase)
               )
            {
                model.SessionToken = paratikaResponse["sessionToken"].ToString();
                _session.Set<string>("SESSIONTOKEN_" + _workContext.CurrentCustomer.Id, paratikaResponse["sessionToken"].ToString());
                return View("~/Plugins/Payments.Paratika/Views/PaymentInfo.cshtml", model);
            }
            else
            {
                model.Error = paratikaResponse.GetValue("errorCode").ToString() + "-" + paratikaResponse.GetValue("errorMsg").ToString();
                model.Error += _localizationService.GetResource("Plugins.Payments.Paratika.Fields.Error");
                return View("~/Plugins/Payments.Paratika/Views/PaymentInfo.cshtml", model);
            }
        }
    }
}
