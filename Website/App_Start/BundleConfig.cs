using System;
using System.Web.Optimization;

namespace TvbsQuiz.Website
{
	public sealed class BundleConfig
	{
		private BundleConfig()
		{
		}

		public static void RegisterBundles(BundleCollection bundles)
		{
			if (bundles == null)
				throw new ArgumentNullException(nameof(bundles));

			bundles.Add(new StyleBundle("~/bundles/css").Include(
				"~/content/styles/master.min.css"));

			bundles.Add(new ScriptBundle("~/bundles/js").Include(
				"~/content/scripts/app.module.js",
				"~/content/scripts/callout-error.component.js",
				"~/content/scripts/evaluation-owner.component.js",
				"~/content/scripts/evaluation-owner.service.js",
				"~/content/scripts/evaluation-player.component.js",
				"~/content/scripts/evaluation-player.service.js",
				"~/content/scripts/evaluation-presenter.component.js",
				"~/content/scripts/evaluation-presenter.service.js",
				"~/content/scripts/utilities.js"));

			BundleTable.EnableOptimizations = Boolean.Parse(System.Configuration.ConfigurationManager.AppSettings["EnableOptimizations"]);
		}
	}
}