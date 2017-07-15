using System;

namespace TvbsQuiz.Domain
{
	public class QuestionOption
	{
		public Boolean IsCorrect { get; set; }

		public Char Letter { get; set; }

		public String Text { get; set; }
	}
}