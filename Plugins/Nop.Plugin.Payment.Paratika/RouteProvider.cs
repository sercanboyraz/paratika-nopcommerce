using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Nop.Web.Framework.Localization;
using Nop.Web.Framework.Mvc.Routing;
using System;
using System.Collections.Generic;
using System.Text;

namespace Nop.Plugin.Payment.Paratika
{
    public class RouteProvider : IRouteProvider
    {
        public void RegisterRoutes(IRouteBuilder routeBuilder)
        {
            routeBuilder.MapLocalizedRoute("Plugin.Payment.Paratika.PaymentInfoCallBack", "Plugins/Paratika/PaymentInfoCallBack",
                new { controller = "Paratika", action = "PaymentInfoCallBack" });
            routeBuilder.MapLocalizedRoute("ConfirmParatikaPayment", "Plugins/Paratika/ConfirmParatikaPayment",
                new { controller = "Paratika", action = "ConfirmParatikaPayment" });
        }

        public int Priority
        {
            get { return -1; }
        }
    }
}