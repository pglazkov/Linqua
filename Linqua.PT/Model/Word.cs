using System;

namespace Linqua.Model
{
	public class Word
	{
		public Word(string text)
			: this(text, DateTime.Now)
		{
		}

		public Word(string text, DateTime dateAdded)
		{
			Text = text;
			DateAdded = dateAdded;
		}

		public string Text { get; private set; }

		public DateTime DateAdded { get; private set; }
	}
}