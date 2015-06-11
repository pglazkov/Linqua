using System;
using System.Threading.Tasks;
using Framework;
using Framework.PlatformServices;
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

				UpdateIsLearntAsync(value).FireAndForget();
			}
		}

		public string IsLearnStatusText
		{
			get
			{
				var result = IsLearnt ? Resources.GetString("EntryViewModel_LearntStatus") : Resources.GetString("EntryViewModel_NotLearntStatus");

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

		private async Task UpdateIsLearntAsync(bool value)
		{
			bool confirmed;

			if (value)
			{
				confirmed = await DialogService.ShowConfirmation(
					Resources.GetString("EntryViewModel_MarkLearnedConfirmationTitle"),
					Resources.GetString("EntryViewModel_MarkLearnedConfirmationText"),
					okCommandText: Resources.GetString("EntryViewModel_MarkLearnedConfirmationOk"));
			}
			else
			{
				confirmed = await DialogService.ShowConfirmation(
					Resources.GetString("EntryViewModel_UnMarkLearnedConfirmationTitle"),
					Resources.GetString("EntryViewModel_UnMarkLearnedConfirmationText"),
					okCommandText: Resources.GetString("EntryViewModel_UnMarkLearnedConfirmationOk"));
			}

			if (confirmed)
			{
				Entry.IsLearnt = value;
				ApplicationController.OnIsLearntChanged(this);
			}

			RaisePropertyChanged(() => IsLearnt);
			RaisePropertyChanged(() => IsLearnStatusText);
		}

		protected virtual void OnDeleted()
		{
		}

		protected virtual void OnEntryChangedOverride()
		{

		}
	}
}