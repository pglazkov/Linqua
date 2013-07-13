using System;
using Framework;
using Linqua.Model;

namespace Linqua
{
	public class WordListItemViewModel : WordViewModel
	{
		public WordListItemViewModel()
		{
			if (DesignTimeDetection.IsInDesignTool)
			{
				Word = new Word("Aankomst", DateTime.Now);
			}
		}

		public WordListItemViewModel(Word word) : base(word)
		{
		}
	}
}