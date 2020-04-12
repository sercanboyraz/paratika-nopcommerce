using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
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
            routeBuilder.MapRoute(
                "Plugin.Payment.Paratika.ParatikaPaymentInfo",
                "Paratika/ParatikaPaymentInfo",
                new { controller = "Paratika", action = "ParatikaPaymentInfo" }
            );

            routeBuilder.MapRoute(
                "Plugin.Payment.Paratika.ParatikaPaymentInfoThreeD",
                "Paratika/ParatikaPaymentInfo3D",
                new { controller = "Paratika", action = "ParatikaPaymentInfo3D" }
            );

            routeBuilder.MapRoute(
                "Plugin.Payment.Paratika.ParatikaInstallment",
                "Paratika/ParatikaInstallment",
                new { controller = "Paratika", action = "ParatikaInstallment" }
            );

            routeBuilder.MapRoute(
                "Plugin.Payment.Paratika.PaymentInfoCallBack",
                "Paratika/PaymentInfoCallBack",
                new { controller = "Paratika", action = "PaymentInfoCallBack" }
            );
            
        }

        public int Priority
        {
            get { return -1; }
        }
    }
}