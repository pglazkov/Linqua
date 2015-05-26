using System;
using System.Collections;
using System.Collections.Generic;
using System.Windows.Input;
using Framework;
using Linqua.DataObjects;
using Linqua.ViewModels.Behaviors;

namespace Linqua
{
	public class EntryListItemViewModel : EntryViewModel
	{
		private bool isTranslationShown;

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

		public IEntryListItemView View { get; set; }

		public bool JustAdded { get; set; }

		public bool IsTranslationShown
		{
			get { return isTranslationShown; }
			set
			{
				if (value == isTranslationShown) return;
				isTranslationShown = value;
				RaisePropertyChanged();
			}
		}

		public void Focus()
		{
			if (View != null)
			{
				View.Focus();
			}
		}
	}
}