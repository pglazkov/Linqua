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

		public EntryListItemViewModel(ClientEntry entry, bool justAdded = false) : base(entry)
		{
			JustAdded = justAdded;
		}

		public IEntryListViewModelView View { get; set; }

		public bool JustAdded { get; set; }

		public void Focus()
		{
			if (View != null)
			{
				View.Focus();
			}
		}
	}
}