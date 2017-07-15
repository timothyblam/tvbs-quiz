using System;
using System.Web.Mvc;
using System.Web.Routing;

namespace TvbsQuiz.Website
{
	public static class RouteConfig
	{
		public static void RegisterRoutes(RouteCollection routes)
		{
			routes.IgnoreRoute("{resource}.axd/{*pathInfo}");

			routes.MapMvcAttributeRoutes();
		}
	}
}