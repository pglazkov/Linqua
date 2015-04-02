using System;
using Framework;
using Linqua.DataObjects;
using Linqua.Events;

namespace Linqua
{
	public class EntryViewModel : ViewModelBase
	{
		private bool isTranslating;

		protected EntryViewModel()
		{
			DeleteCommand = new DelegateCommand(DeleteSelf);
		}

		public EntryViewModel(ClientEntry entry)
			: this()
		{
			Guard.NotNull(entry, () => entry);

			Entry = entry;
		}


		public DelegateCommand DeleteCommand { get; private set; }

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

		public bool IsTranslating
		{
			get { return isTranslating; }
			set
			{
				if (value.Equals(isTranslating)) return;
				isTranslating = value;
				RaisePropertyChanged();
				RaisePropertyChanged(() => IsDefinitionVisible);
			}
		}

		public string Definition
		{
			get { return Entry.Definition; }
			set
			{
				if (Equals(Definition, value))
				{
					return;
				}

				Entry.Definition = value;
				RaisePropertyChanged();
				RaisePropertyChanged(() => IsDefinitionVisible);

				EventAggregator.Publish(new EntryDefinitionChangedEvent(Entry));
			}
		}

		public bool IsDefinitionVisible
		{
			get { return !string.IsNullOrEmpty(Definition) || IsTranslating; }
		}

		private void DeleteSelf()
		{
			EventAggregator.Publish(new EntryDeletionRequestedEvent(Entry));
		}
	}
}