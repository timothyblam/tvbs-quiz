using System;
using System.Web.Mvc;
using TvbsQuiz.Domain;
using TvbsQuiz.Website.WebSocketHandlers;

namespace TvbsQuiz.Website.Controllers
{
	
	public class EvaluationController : Controller
	{
		[Route("owner")]
		public ActionResult Owner()
		{
			this.ViewBag.Color = Global.Evaluation.Color;
			this.ViewBag.Id = Global.Evaluation.Id;
			this.ViewBag.Team = Global.Evaluation.Team;

			return this.View();
		}

		[Route("")]
		[Route("player")]
		public ActionResult Player()
		{
			this.ViewBag.Color = Global.Evaluation.Color;
			this.ViewBag.Id = Global.Evaluation.Id;

			return this.View();
		}

		[Route("presenter")]
		public ActionResult Presenter()
		{
			this.ViewBag.Color = Global.Evaluation.Color;
			this.ViewBag.Id = Global.Evaluation.Id;

			return this.View();
		}

		[Route("reset/{evaluationId}")]
		public ActionResult Reset(Int32 evaluationId)
		{
			EvaluationWebSocketHandler.Current = new EvaluationWebSocketHandler();
			Global.Evaluation = new Evaluation(evaluationId, this.Server.MapPath("~/App_Data"));

			return this.Redirect("~/presenter");
		}
	}
}