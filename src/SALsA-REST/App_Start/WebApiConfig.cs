using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http;

namespace SALsA_REST
{
    public static class WebApiConfig
    {
        public static void Register(HttpConfiguration config)
        {
            // Web API configuration and services

            // Web API routes
            config.MapHttpAttributeRoutes();

            config.Routes.MapHttpRoute(
                name: "Status",
                routeTemplate: "api/icm/status/{id}",
                defaults: new { controller = "ICMStatus", action = "Get", id = RouteParameter.Optional }
            );

            config.Routes.MapHttpRoute(
                name: "DefaultApi",
                routeTemplate: "api/{controller}/{id}",
                defaults: new { id = RouteParameter.Optional }
            );

            config.Routes.MapHttpRoute(
                name: "ManualRunIID",
                routeTemplate: "manualrun/iid",
                defaults: new { controller = "ManualRunIID", id = RouteParameter.Optional }
            );

            config.Routes.MapHttpRoute(
                name: "ManualRunICM",
                routeTemplate: "manualrun/icm",
                defaults: new { controller = "ManualRunICM", id = RouteParameter.Optional }
            );

            config.Routes.MapHttpRoute(
                name: "ManualRunRdfeFabric",
                routeTemplate: "manualrun/rdfe/fabric",
                defaults: new { controller = "ManualRunRdfeFabric", id = RouteParameter.Optional }
            );

            config.Routes.MapHttpRoute(
                name: "ManualRunRdfeTenant",
                routeTemplate: "manualrun/rdfe/tenant",
                defaults: new { controller = "ManualRunRdfeTenant", id = RouteParameter.Optional }
            );
        }
    }
}
