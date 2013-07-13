using System;
using Framework;
using Linqua.Model;

namespace Linqua
{
	public class WordViewModel : ViewModelBase
	{
		protected WordViewModel()
		{
			
		}

		public WordViewModel(Word word)
		{
			Guard.NotNull(() => word);

			Word = word;
		}

		protected Word Word { get; set; }

		public string Text
		{
			get { return Word.Text; }
		}

		public DateTime DateAdded
		{
			get { return Word.DateAdded; }
		}
	}
}