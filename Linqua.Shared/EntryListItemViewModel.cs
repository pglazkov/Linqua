using System;
using Framework;
using Linqua.DataObjects;

namespace Linqua
{
	public class EntryListItemViewModel : EntryViewModel
	{
		public EntryListItemViewModel()
		{
			if (DesignTimeDetection.IsInDesignTool)
			{
				Entry = new ClientEntry("Aankomst");
			}
		}

		public EntryListItemViewModel(ClientEntry entry) : base(entry)
		{
		}
	}
}