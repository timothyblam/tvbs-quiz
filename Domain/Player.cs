using System;

namespace TvbsQuiz.Domain
{
	public class Player : User
	{
		public String Code { get; set; }

		public Boolean IsActive { get; set; } = true;

		public Boolean IsConnected { get; set; } = false;

		public String Name { get; set; }

		public Int32 Points { get; set; } = 0;

		public Int32 TotalPoints { get; set; } = 0;
	}
}