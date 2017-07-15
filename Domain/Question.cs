using System;
using System.Collections.ObjectModel;

namespace TvbsQuiz.Domain
{
	public class Question
	{
		public Collection<Answer> Answers { get; } = new Collection<Answer>();

		public Collection<QuestionOption> Options { get; } = new Collection<QuestionOption>();

		public String Text { get; set; }

		public Question(String text)
		{
			this.Text = text;
		}
	}
}