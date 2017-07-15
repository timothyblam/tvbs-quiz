using System;
using System.Net.WebSockets;
using Newtonsoft.Json;

namespace TvbsQuiz.Domain
{
	public class User
	{
		public Guid Uid { get; } = Guid.NewGuid();

		[JsonIgnore]
		public WebSocket WebSocket { get; set; }
	}
}