using System;
using System.Net;
using System.Web;
using System.Web.Http;

using TvbsQuiz.Website.WebSocketHandlers;

namespace TvbsQuiz.Website.Api
{
	[RoutePrefix("api/evaluations")]
	public class EvaluationApiController : ApiController
	{
		[HttpGet]
		[Route("websockets")]
		public IHttpActionResult CreateEvaluationWebSocket()
		{
			HttpContext.Current.AcceptWebSocketRequest(EvaluationWebSocketHandler.Current.ReceiveLoopAsync);

			return this.StatusCode(HttpStatusCode.SwitchingProtocols);
		}
	}
}