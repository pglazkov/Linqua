using System;
using Framework;
using Linqua.DataObjects;

namespace Linqua
{
	public class WordListItemViewModel : WordViewModel
	{
		public WordListItemViewModel()
		{
			if (DesignTimeDetection.IsInDesignTool)
			{
				Entry = new ClientEntry("Aankomst", DateTime.Now);
			}
		}

		public WordListItemViewModel(ClientEntry entry) : base(entry)
		{
		}
	}
}