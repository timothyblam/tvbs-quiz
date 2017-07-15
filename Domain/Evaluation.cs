using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Net.WebSockets;

using Newtonsoft.Json.Linq;

namespace TvbsQuiz.Domain
{
	public class Evaluation
	{
		#region Properties

		public String Color { get; set; }

		public Question CurrentQuestion
		{
			get
			{
				if(this.CurrentQuestionIndex == -1)
					return null;
				else
					return this.Questions[this.CurrentQuestionIndex];
			}
		}

		private Int32 CurrentQuestionIndex { get; set; } = -1;

		public Int32 CurrentQuestionNumber
		{
			get
			{
				return this.CurrentQuestionIndex + 1;
			}
		}

		public String FilePath { get; set; }

		public Int32 Id { get; private set; }

		public Object Lock { get; set; } = new Object();

		public Collection<Owner> Owners { get; } = new Collection<Owner>();

		public Collection<WebSocket> OwnerWebSockets
		{
			get
			{
				Collection<WebSocket> webSockets = new Collection<WebSocket>();

				foreach(Owner owner in this.Owners)
					webSockets.Add(owner.WebSocket);

				return webSockets;
			}
		}
		
		public Collection<Player> Players { get; } = new Collection<Player>();

		public Collection<WebSocket> PlayerWebSockets
		{
			get
			{
				Collection<WebSocket> webSockets = new Collection<WebSocket>();

				foreach (Player player in this.Players)
					webSockets.Add(player.WebSocket);

				return webSockets;
			}
		}

		public Collection<Presenter> Presenters { get; } = new Collection<Presenter>();

		public Collection<WebSocket> PresenterWebSockets
		{
			get
			{
				Collection<WebSocket> webSockets = new Collection<WebSocket>();

				foreach (Presenter presenter in this.Presenters)
					webSockets.Add(presenter.WebSocket);

				return webSockets;
			}
		}

		public Collection<Question> Questions { get; } = new Collection<Question>();

		public EvaluationState State { get; set; }

		public String Team { get; set; }

		#endregion Properties

		#region Constructors

		public Evaluation(Int32 id, String filePath)
		{
			this.Id = id;
			this.FilePath = filePath;
			this.State = EvaluationState.None;

			this.LoadColor();
			this.LoadQuestions();
			this.LoadPlayers();
			this.LoadTeam();
		}

		#endregion Constructors


		public Owner AddOwner(WebSocket webSocket)
		{
			Owner owner = new Owner()
			{
				WebSocket = webSocket
			};

			lock (this.Lock)
			{
				this.Owners.Add(owner);
			}

			return owner;
		}

		public Presenter AddPresenter(WebSocket webSocket)
		{
			Presenter presenter = new Presenter()
			{
				WebSocket = webSocket
			};

			lock (this.Lock)
			{
				this.Presenters.Add(presenter);
			}

			return presenter;
		}

		public Boolean DisconnectOwner(WebSocket webSocket)
		{
			Boolean disconnected = false;

			lock (this.Lock)
			{
				for (Int32 i = 0; i < this.Owners.Count; ++i)
				{
					if (this.Owners[i].WebSocket == webSocket)
					{
						this.Owners.RemoveAt(i);
						disconnected = true;
					}
				}
			}

			return disconnected;
		}

		public User DisconnectPlayer(WebSocket webSocket)
		{
			foreach (Player player in this.Players)
			{
				if (webSocket == player.WebSocket)
				{
					player.IsConnected = false;
					player.WebSocket = null;

					return player;
				}
			}

			return null;
		}

		public Boolean DisconnectPresenter(WebSocket webSocket)
		{
			Boolean disconnected = false;

			lock (this.Lock)
			{
				for (Int32 i = 0; i < this.Presenters.Count; ++i)
				{
					if (this.Presenters[i].WebSocket == webSocket)
					{
						this.Presenters.RemoveAt(i);
						disconnected = true;
					}
				}
			}

			return disconnected;
		}

		public Player MatchPlayer(Guid uid)
		{
			foreach (Player player in this.Players)
				if (player.Uid == uid)
					return player;

			return null;
		}

		public Player MatchPlayer(WebSocket webSocket, String code)
		{
			foreach (Player player in this.Players)
			{
				if (player.Code == code)
				{
					player.WebSocket = webSocket;

					return player;
				}
			}

			return null;
		}

		public void LoadColor()
		{
			this.Color = (String)JObject.Parse(File.ReadAllText(FormattableString.Invariant($@"{ this.FilePath }/{ this.Id }.json")))["color"];
		}

		public void LoadQuestions()
		{
			IEnumerable<Question> questions = JObject.Parse(File.ReadAllText(FormattableString.Invariant($@"{ this.FilePath }/{ this.Id }.json")))["questions"].ToObject<IEnumerable<Question>>();

			foreach (Question question in questions)
				this.Questions.Add(question);
		}

		public void LoadPlayers()
		{
			IEnumerable<Player> players = JObject.Parse(File.ReadAllText(FormattableString.Invariant($@"{ this.FilePath }/{ this.Id }.json")))["players"].ToObject<IEnumerable<Player>>();

			foreach (Player player in players)
				this.Players.Add(player);
		}

		public void LoadTeam()
		{
			this.Team = (String)JObject.Parse(File.ReadAllText(FormattableString.Invariant($@"{ this.FilePath }/{ this.Id }.json")))["team"];
		}

		public Question MoveToNextQuestion()
		{
			++this.CurrentQuestionIndex;

			return this.CurrentQuestion;
		}

		public User RemoveUser(WebSocket webSocket)
		{
			foreach (Player player in this.Players)
			{
				if (webSocket == player.WebSocket)
				{
					this.Players.Remove(player);

					return player;
				}
			}

			foreach (Owner owner in this.Owners)
			{
				if (webSocket == owner.WebSocket)
				{
					this.Owners.Remove(owner);

					return owner;
				}
			}

			return null;
		}

		public void Restart()
		{
			this.CurrentQuestionIndex = -1;
			this.State = EvaluationState.None;

			foreach (Player player in this.Players)
				player.Points = 0;
		}
	}
}