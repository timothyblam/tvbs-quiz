using System;
using System.Web.Mvc;
using TvbsQuiz.Website.ErrorHandlers;

namespace TvbsQuiz.Website
{
	public static class FilterConfig
	{
		public static void RegisterGlobalFilters(GlobalFilterCollection filters)
		{
			if (filters == null)
				throw new ArgumentNullException(nameof(filters));

			filters.Add(new AiHandleErrorAttribute());
		}
	}
}