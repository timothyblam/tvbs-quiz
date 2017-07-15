using System;
using System.Collections.ObjectModel;
using System.Dynamic;
using System.Net.WebSockets;
using System.Threading.Tasks;

using Advantage.Framework.WebSockets;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;
using TvbsQuiz.Domain;

namespace TvbsQuiz.Website.WebSocketHandlers
{
	public class EvaluationWebSocketHandler : WebSocketHandler
	{
		#region Properties

		/// <summary>
		/// Gets a reference to the current Evaluation WebSocket handler.
		/// </summary>
		public static EvaluationWebSocketHandler Current { get; set; } = new EvaluationWebSocketHandler();

		#endregion Properties

		#region Constructors

		/// <summary>
		/// Default constructor.
		/// </summary>
		public EvaluationWebSocketHandler()
		{
			this.ConnectionClosed += this.EvaluationWebSocketHandler_ConnectionClosed;
			this.MessageReceived += this.EvaluationWebSocketHandler_MessageReceived;
		}

		#endregion Constructors

		#region Events

		private void EvaluationWebSocketHandler_ConnectionClosed(Object sender, EventArgs e)
		{
			WebSocket webSocket = (WebSocket)sender;
			Player player = (Player)Global.Evaluation.DisconnectPlayer(webSocket);

			// If a player's web socket connection closed, notify the owners.
			if (player != null)
			{
				dynamic response = new ExpandoObject();
				response.Player = player;

				Task.Run(() => this.SendAsync(response, Global.Evaluation.OwnerWebSockets, "playerDisconnected")).Wait();
			}
			else
			{
				if (!Global.Evaluation.DisconnectOwner(webSocket))
					Global.Evaluation.DisconnectPresenter(webSocket);
			}
		}

		private void EvaluationWebSocketHandler_MessageReceived(Object sender, WebSocketEventArgs e)
		{
			WebSocket webSocket = (WebSocket)sender;

			if (e.Json.Value<String>("sender") == "player")
				Task.Run(() => this.PlayerMessageReceivedAsync(e.Json.Value<JObject>("data"), webSocket, e.Json.Value<String>("action"))).Wait();
			else if (e.Json.Value<String>("sender") == "presenter")
				Task.Run(() => this.PresenterMessageReceivedAsync(e.Json.Value<JObject>("data"), webSocket, e.Json.Value<String>("action"))).Wait();
			else
				Task.Run(() => this.OwnerMessageReceivedAsync(e.Json.Value<JObject>("data"), webSocket, e.Json.Value<String>("action"))).Wait();
		}

		#endregion Events

		#region Receive

		private async Task OwnerMessageReceivedAsync(JObject data, WebSocket webSocket, String action)
		{
			switch (action)
			{
				case "connect":
					Owner owner = Global.Evaluation.AddOwner(webSocket);

					dynamic connectResponseData = new ExpandoObject();
					connectResponseData.Players = Global.Evaluation.Players;
					connectResponseData.Question = Global.Evaluation.CurrentQuestion;
					connectResponseData.QuestionNumber = Global.Evaluation.CurrentQuestionNumber;
					connectResponseData.Uid = owner.Uid;

					await this.SendAsync(connectResponseData, webSocket, "connected");
					break;
				case "restart":
					Global.Evaluation.Restart();
					await this.SendAllAsync(null, "restarted");
					break;
				case "showResults":
					Global.Evaluation.State = EvaluationState.Results;

					await this.SendAsync(null, Global.Evaluation.PlayerWebSockets, "showResult");
					break;
				case "startNextQuestion":
					Global.Evaluation.State = EvaluationState.Question;

					dynamic startNextQuestionResponseData = new ExpandoObject();
					startNextQuestionResponseData.Question = Global.Evaluation.MoveToNextQuestion();
					startNextQuestionResponseData.QuestionNumber = Global.Evaluation.CurrentQuestionNumber;

					await this.SendAllAsync(startNextQuestionResponseData, "nextQuestionStarted");
					break;
			}
		}

		private async Task PlayerMessageReceivedAsync(JObject data, WebSocket webSocket, String action)
		{
			switch (action)
			{
				case "connect":
					Player player = Global.Evaluation.MatchPlayer(webSocket, data.Value<String>("code"));

					if (player == null)
						await this.SendAsync(null, webSocket, "notConnected");
					else
					{
						player.IsConnected = true;

						dynamic playerResponseData = new ExpandoObject();
						playerResponseData.Name = player.Name;
						playerResponseData.Points = player.Points;

						if (Global.Evaluation.State == EvaluationState.Question)
						{
							playerResponseData.Question = Global.Evaluation.CurrentQuestion;
							playerResponseData.QuestionNumber = Global.Evaluation.CurrentQuestionNumber;
						}
						else
						{
							playerResponseData.Question = null;
							playerResponseData.QuestionNumber = 0;
						}

						playerResponseData.Uid = player.Uid;

						dynamic ownersResponseData = new ExpandoObject();
						ownersResponseData.Player = player;

						await Task.WhenAll(new Task[]
						{
							this.SendAsync(playerResponseData, webSocket, "connected"),
							this.SendAsync(ownersResponseData, Global.Evaluation.OwnerWebSockets, "playerConnected")
						});
					}
					break;
				case "submitAnswer":
					Boolean isCorrect = false;
					Char letter = data.Value<Char>("answer");

					player = Global.Evaluation.MatchPlayer(Guid.Parse(data.Value<String>("uid")));

					foreach (QuestionOption option in Global.Evaluation.CurrentQuestion.Options)
						if (option.Letter.Equals(letter) && option.IsCorrect)
							isCorrect = true;

					if (isCorrect)
						player.Points += 100;

					dynamic ownersSubmitAnswerResponseData = new ExpandoObject();
					ownersSubmitAnswerResponseData.Answer = new ExpandoObject();
					ownersSubmitAnswerResponseData.Answer.IsCorrect = isCorrect;
					ownersSubmitAnswerResponseData.Answer.Letter = letter;
					ownersSubmitAnswerResponseData.PlayerUid = data.Value<String>("uid");
					await this.SendAsync(ownersSubmitAnswerResponseData, Global.Evaluation.OwnerWebSockets, "answerSubmitted");

					await this.SendAsync(null, webSocket, "answerSubmitted");
					break;
			}
		}

		private async Task PresenterMessageReceivedAsync(JObject data, WebSocket webSocket, String action)
		{
			switch (action)
			{
				case "connect":
					Presenter presenter = Global.Evaluation.AddPresenter(webSocket);

					dynamic connectResponseData = new ExpandoObject();
					connectResponseData.State = Global.Evaluation.State == EvaluationState.Question ? "question" : "players";
					connectResponseData.Uid = presenter.Uid;

					await this.SendAsync(connectResponseData, webSocket, "connected");
					break;
				case "restart":
					Global.Evaluation.Restart();
					await this.SendAllAsync(null, "restarted");
					break;
				case "showResults":
					Global.Evaluation.State = EvaluationState.Results;

					await this.SendAsync(null, Global.Evaluation.OwnerWebSockets, "showResults");
					await this.SendAsync(null, Global.Evaluation.PlayerWebSockets, "showResult");
					break;
				case "startNextQuestion":
					if (Global.Evaluation.CurrentQuestionNumber < Global.Evaluation.Questions.Count)
					{
						Global.Evaluation.State = EvaluationState.Question;

						dynamic startNextQuestionResponseData = new ExpandoObject();
						startNextQuestionResponseData.Question = Global.Evaluation.MoveToNextQuestion();
						startNextQuestionResponseData.QuestionNumber = Global.Evaluation.CurrentQuestionNumber;

						await this.SendAllAsync(startNextQuestionResponseData, "nextQuestionStarted");
					}
					else
						await this.SendAsync(null, Global.Evaluation.OwnerWebSockets, "showTotals");
					break;
			}
		}

		#endregion Receive

		#region Send

		private async Task SendAsync(dynamic data, WebSocket webSocket, String action)
		{
			await this.SendAsync(data, new Collection<WebSocket>() { webSocket }, action);
		}

		private async Task SendAsync(dynamic data, Collection<WebSocket> webSockets, String action)
		{
			dynamic response = new ExpandoObject();
			response.Action = action;
			response.Data = data;

			await this.SendAsync(JsonConvert.SerializeObject(response, new JsonSerializerSettings() { ContractResolver = new CamelCasePropertyNamesContractResolver() }), webSockets);
		}

		private async Task SendAllAsync(dynamic data, String action)
		{
			dynamic response = new ExpandoObject();
			response.Action = action;
			response.Data = data;

			await this.SendAllAsync(JsonConvert.SerializeObject(response, new JsonSerializerSettings() { ContractResolver = new CamelCasePropertyNamesContractResolver() }));
		}

		#endregion Send
	}
}