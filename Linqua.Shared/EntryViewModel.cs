﻿using System;
using Framework;
using Linqua.DataObjects;
using Linqua.Events;

namespace Linqua
{
	public class EntryViewModel : ViewModelBase
	{
		protected EntryViewModel()
		{
			
		}

		public EntryViewModel(ClientEntry entry)
		{
			Guard.NotNull(entry, () => entry);

			Entry = entry;
		}

		public ClientEntry Entry { get; protected set; }

		public string Text
		{
			get { return Entry.Text; }
		}

		public DateTime DateAdded
		{
			get { return Entry.CreatedAt.GetValueOrDefault().DateTime; }
		}

		public bool IsLearnt
		{
			get { return Entry.IsLearnt; }
			set
			{
				if (Equals(IsLearnt, value))
				{
					return;
				}

				Entry.IsLearnt = value;
				RaisePropertyChanged();

				EventAggregator.Publish(new EntryIsLearntChangedEvent(this));
			}
		}
	}
}