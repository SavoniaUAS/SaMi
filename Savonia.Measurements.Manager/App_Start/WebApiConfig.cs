using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http;
using Savonia.Measurements.Models;
using Savonia.Measurements.Database;
using System.Web.OData.Builder;
using System.Web.OData.Extensions;


namespace Savonia.Measurements.Manager
{
    public static class WebApiConfig
    {
        public static void Register(HttpConfiguration config)
        {
            // Web API configuration and services

            // Web API routes
            config.MapHttpAttributeRoutes();

            config.Routes.MapHttpRoute(
                name: "DefaultApi",
                routeTemplate: "api/{controller}/{key}",
                defaults: new { key = RouteParameter.Optional }
            );

            // OData routes
            ODataModelBuilder builder = new ODataConventionModelBuilder();
            
            var es = builder.EntitySet<MeasurementModel>("Measurements");
            es.EntityType.HasKey(m => m.Key);

            config.MapODataServiceRoute(
                routeName: "odata", 
                routePrefix: "odata/V4", 
                model: builder.GetEdmModel());
            config.AddODataQueryFilter();       
        }
    }
}
