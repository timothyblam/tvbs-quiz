using System;
using System.Web.Http;

namespace TvbsQuiz.Website
{
	public static class WebApiConfig
	{
		public static void Register(HttpConfiguration config)
		{
			config.MapHttpAttributeRoutes();
		}
	}
}