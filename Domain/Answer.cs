using System;

namespace TvbsQuiz.Domain
{
	public class Answer
	{
		public Boolean IsCorrect { get; set; } = false;

		public Char Letter { get; set; }

		public Player Player { get; set; }

		public Question Question { get; set; }
	}
}