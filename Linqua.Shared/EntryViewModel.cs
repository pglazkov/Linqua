using System;
using Framework;
using Framework.PlatformServices;
using JetBrains.Annotations;
using Linqua.DataObjects;
using Linqua.Events;

namespace Linqua
{
	public class EntryViewModel : ViewModelBase
	{
		private bool isTranslating;
		private ClientEntry entry;

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

		private IApplicationController ApplicationController
		{
			get { return CompositionFactory.Create<IApplicationController>(); }
		}

		public ClientEntry Entry
		{
			get { return entry; }
			protected set
			{
				if (Equals(value, entry)) return;
				entry = value;
				RaisePropertyChanged();
				RaisePropertyChanged(() => Text);
				RaisePropertyChanged(() => DateAdded);
				RaisePropertyChanged(() => IsLearnt);
				RaisePropertyChanged(() => IsLearnStatusText);
				RaisePropertyChanged(() => Definition);
				RaisePropertyChanged(() => IsDefinitionVisible);
				OnEntryChangedOverride();
			}
		}

		public string Text
		{
			get { return Entry != null ? Entry.Text : string.Empty; }
		}

		public DateTime DateAdded
		{
			get { return Entry != null ? Entry.CreatedAt.GetValueOrDefault().DateTime : DateTime.MinValue; }
		}

		public bool IsLearnt
		{
			get { return Entry != null && Entry.IsLearnt; }
			set
			{
				if (Equals(IsLearnt, value))
				{
					return;
				}

				Guard.Assert(Entry != null, "Entry != null");

				Entry.IsLearnt = value;
				RaisePropertyChanged();
				RaisePropertyChanged(() => IsLearnStatusText);

				ApplicationController.OnIsLearntChanged(this);
			}
		}

		public string IsLearnStatusText
		{
			get
			{
				var res = CompositionFactory.Create<IStringResourceManager>();

				var result = IsLearnt ? res.GetString("EntryViewModel_LearntStatus") : res.GetString("EntryViewModel_NotLearntStatus");

				return result;
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
			get { return Entry != null ? Entry.Definition : string.Empty; }
			set
			{
				if (Equals(Definition, value))
				{
					return;
				}

				Guard.Assert(Entry != null, "Entry != null");

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
			Guard.Assert(Entry != null, "Entry != null");

			EventAggregator.Publish(new EntryDeletionRequestedEvent(Entry));

			OnDeleted();
		}

		protected virtual void OnDeleted()
		{
		}

		protected virtual void OnEntryChangedOverride()
		{

		}
	}
}