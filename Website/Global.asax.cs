using System;
using System.Web;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;
using System.Web.Http;

using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using TvbsQuiz.Domain;

[assembly: CLSCompliant(true)]
namespace TvbsQuiz.Website
{
	public class Global : HttpApplication
	{
		public static Evaluation Evaluation { get; set; } = new Evaluation(1, HttpContext.Current.Server.MapPath("~/App_Data"));

		public void Application_Start(Object sender, EventArgs e)
		{
			AreaRegistration.RegisterAllAreas();
			GlobalConfiguration.Configure(WebApiConfig.Register);
			RouteConfig.RegisterRoutes(RouteTable.Routes);
			BundleConfig.RegisterBundles(BundleTable.Bundles);

			// Set Web API to use lower camel case when serializing JSON.
			var formatters = GlobalConfiguration.Configuration.Formatters;
			var jsonFormatter = formatters.JsonFormatter;
			var settings = jsonFormatter.SerializerSettings;
			settings.Formatting = Formatting.Indented;
			settings.ContractResolver = new CamelCasePropertyNamesContractResolver();
		}
	}
}